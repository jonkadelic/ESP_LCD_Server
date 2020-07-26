using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using SpotifyAPI.Web;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.IO;
using SuperfastBlur;
using LCDWidget;
using System.Linq;

namespace SpotifyWidget
{
    public class SpotifyWidget : BaseNotifyingWidget
    {
        private readonly string clientToken = Secrets.SpotifyClientToken;
        private string accessToken = null;
        private readonly string refreshToken = Secrets.SpotifyRefreshToken;
        private SpotifyClient client = null;
        private DateTime nextAccessTokenRefresh;
        private CurrentlyPlaying cachedCurrentlyPlaying = null;
        private const int updatePeriodMs = 5000;
        private Bitmap artImage;
        private Bitmap artImageBlurred;
        private string songName;
        private string albumName;
        private string artistName;
        private Font smallFont = new Font(FontFamily.GenericSansSerif, 7);
        private Font bigFont = new Font(FontFamily.GenericSansSerif, 10);
        private StringFormat stringFormat = new StringFormat() { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        private StringFormat pauseFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        private bool renderArtBig = false;
        private Rectangle artSmallRectangle;
        private Rectangle artLargeRectangle;
        private DateTime lastUpdate;

        public override string Name => "Spotify";
        public override int NotifyDurationMs => 3000;

        public override int Priority => 1;

        public SpotifyWidget()
        {
            int artSmallDims = (int)(FrameSize.Width * 0.8);
            int artLargeDims = FrameSize.Width;
            artSmallRectangle = new Rectangle(new Point(FrameSize.Width / 2 - artSmallDims / 2, FrameSize.Height / 2 - artSmallDims / 2 + 15), new Size(artSmallDims, artSmallDims));
            artLargeRectangle = new Rectangle(new Point(0, FrameSize.Height / 2 - artLargeDims / 2), new Size(artLargeDims, artLargeDims));
            UpdateTask();
        }

        public override async Task HandleActionAsync()
        {
            await Task.Run(() =>
            {
                renderArtBig = !renderArtBig;
            });
        }

        public override Bitmap RenderFrame()
        {
            Bitmap Frame = new Bitmap(FrameSize.Width, FrameSize.Height);

            Graphics g = Graphics.FromImage(Frame);
            g.FillRectangle(Brushes.Black, new Rectangle(Point.Empty, FrameSize));
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            if (cachedCurrentlyPlaying == null)
            {
                g.DrawString("Nothing is playing on Spotify :(", bigFont, Brushes.White, new Rectangle(Point.Empty, FrameSize), pauseFormat);
                return Frame;
            }

            if (artImageBlurred != null)
            {
                g.DrawImage(artImageBlurred, new Rectangle(-16, 0, FrameSize.Height, FrameSize.Height));
                g.FillRectangle(new SolidBrush(Color.FromArgb(127, Color.Black)), new Rectangle(Point.Empty, FrameSize));
            }

            if (renderArtBig)
            {
                if (artImage != null)
                {
                    g.DrawImage(artImage, artLargeRectangle);
                }
                if (cachedCurrentlyPlaying.IsPlaying == false)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(63, Color.Black)), artLargeRectangle);
                    g.DrawString("Paused", bigFont, Brushes.White, artLargeRectangle, pauseFormat);
                }
            }
            else
            {
                if (artImage != null)
                {
                    g.DrawImage(artImage, artSmallRectangle);
                }
                if (cachedCurrentlyPlaying.IsPlaying == false)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(63, Color.Black)), artSmallRectangle);
                    g.DrawString("Paused", bigFont, Brushes.White, artSmallRectangle, pauseFormat);
                }
                g.DrawString($"{songName}", bigFont, Brushes.Black, new Rectangle(1, 1, FrameSize.Width, 16), stringFormat);
                g.DrawString($"{songName}", bigFont, Brushes.White, new Rectangle(0, 0, FrameSize.Width, 16), stringFormat);
                g.DrawString($"{albumName}\n{artistName}", smallFont, Brushes.Black, new Rectangle(1, 17, FrameSize.Width, 24), stringFormat);
                g.DrawString($"{albumName}\n{artistName}", smallFont, Brushes.White, new Rectangle(0, 16, FrameSize.Width, 24), stringFormat);
            }

            float progress;
            if (cachedCurrentlyPlaying.Item.Type == ItemType.Track)
            {
                FullTrack item = cachedCurrentlyPlaying.Item as FullTrack;
                progress = (float)(cachedCurrentlyPlaying.ProgressMs + (DateTime.Now - lastUpdate).TotalMilliseconds) / item.DurationMs;
            }
            else
            {
                FullEpisode item = cachedCurrentlyPlaying.Item as FullEpisode;
                progress = (float)(cachedCurrentlyPlaying.ProgressMs + (DateTime.Now - lastUpdate).TotalMilliseconds) / item.DurationMs;
            }

            g.DrawImageUnscaled(DrawProgressLine(progress), Point.Empty);

            return Frame;
        }

        public Bitmap DrawProgressLine(float progress)
        {
            int top = 0;
            int bottom = FrameSize.Height - 1;
            int left = 0;
            int right = FrameSize.Width - 1;
            Point lineTopLeft = new Point(left, top);
            Point lineTopRight = new Point(right, top);
            Point lineBottomRight = new Point(right, bottom);
            Point lineBottomLeft = new Point(left, bottom);

            Bitmap bitmap = new Bitmap(FrameSize.Width, FrameSize.Height);
            using Graphics g = Graphics.FromImage(bitmap);

            if (progress <= 0.25)
            {
                g.DrawLine(Pens.White, lineTopLeft, new Point((int)(left + (right - left) * (progress * 4)), top));
            }
            else if (progress <= 0.5)
            {
                g.DrawLine(Pens.White, lineTopLeft, lineTopRight);
                g.DrawLine(Pens.White, lineTopRight, new Point(right, (int)(top + (bottom - top) * ((progress - 0.25) * 4))));
            }
            else if (progress <= 0.75)
            {
                g.DrawLine(Pens.White, lineTopLeft, lineTopRight);
                g.DrawLine(Pens.White, lineTopRight, lineBottomRight);
                g.DrawLine(Pens.White, lineBottomRight, new Point((int)(right + (left - right) * ((progress - 0.5) * 4)), bottom));
            }
            else
            {
                g.DrawLine(Pens.White, lineTopLeft, lineTopRight);
                g.DrawLine(Pens.White, lineTopRight, lineBottomRight);
                g.DrawLine(Pens.White, lineBottomRight, lineBottomLeft);
                g.DrawLine(Pens.White, lineBottomLeft, new Point(left, (int)(bottom + (top - bottom) * ((progress - 0.75) * 4))));
            }

            return bitmap;
        }

        /// <summary>
        /// Passively updates Spotify data in the background at a regular interval.
        /// </summary>
        private async void UpdateTask()
        {
            while (true)
            {
                await UpdateCachedCurrentlyPlaying();
                lastUpdate = DateTime.Now;
                Thread.Sleep(updatePeriodMs);
            }
        }

        /// <summary>
        /// Updates cached Spotify data.
        /// </summary>
        /// <returns>Task status.</returns>
        private async Task UpdateCachedCurrentlyPlaying()
        {
            if (accessToken == null || client == null || DateTime.Now > nextAccessTokenRefresh)
            {
                Logger.Log("Refreshing access token.", this.GetType());
                await RefreshAccessToken();
                client = new SpotifyClient(accessToken);
            }
            CurrentlyPlaying newCurrentlyPlaying = await client.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.All));
            if (newCurrentlyPlaying == null || newCurrentlyPlaying.Item == null)
            {
                cachedCurrentlyPlaying = null;
                return;
            }

            if (cachedCurrentlyPlaying != null)
            {
                if (((newCurrentlyPlaying.Item.Type == ItemType.Track) ? (newCurrentlyPlaying.Item as FullTrack).Id : (newCurrentlyPlaying.Item as FullEpisode).Id) == ((cachedCurrentlyPlaying.Item.Type == ItemType.Track) ? (cachedCurrentlyPlaying.Item as FullTrack).Id : (cachedCurrentlyPlaying.Item as FullEpisode).Id))
                {
                    cachedCurrentlyPlaying = newCurrentlyPlaying;
                    return;
                }
            }
            cachedCurrentlyPlaying = newCurrentlyPlaying;

            string artUrl;

            if (cachedCurrentlyPlaying.Item.Type == ItemType.Track)
            {
                FullTrack item = cachedCurrentlyPlaying.Item as FullTrack;
                songName = item.Name;
                albumName = item.Album.Name;
                artistName = item.Artists.Select((x) => x.Name).Aggregate((x, y) => x + ", " + y);
                artUrl = item.Album.Images[0].Url;
            }
            else
            {
                FullEpisode item = cachedCurrentlyPlaying.Item as FullEpisode;
                songName = item.Name;
                albumName = item.Show.Name;
                artistName = item.Show.Publisher;
                artUrl = item.Show.Images[0].Url;
            }

            using WebClient webClient = new WebClient();
            byte[] data = webClient.DownloadData(artUrl);
            using MemoryStream ms = new MemoryStream(data);
            artImage = (Bitmap)System.Drawing.Image.FromStream(ms);
            artImageBlurred = new GaussianBlur(artImage).Process(20);

            Notification notif = new Notification($"{albumName}\n{artistName}", this, artImage, Notification.ICON_STYLE.SQUARE, NotifyDurationMs, songName);

            OnNotify(notif);
        }

        /// <summary>
        /// Refreshes the Spotify API access token.
        /// </summary>
        /// <returns>Task status.</returns>
        private async Task RefreshAccessToken()
        {
            using HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>() { { "grant_type", "refresh_token" }, { "refresh_token", refreshToken } })
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", clientToken);

            HttpResponseMessage message = await client.SendAsync(request);
            string response = await message.Content.ReadAsStringAsync();

            if (message.IsSuccessStatusCode)
            {
                JObject json = JObject.Parse(response);
                accessToken = json["access_token"].Value<string>();
                int durationSeconds = json["expires_in"].Value<int>();
                nextAccessTokenRefresh = DateTime.Now + new TimeSpan(0, 0, durationSeconds);
            }
        }
    }
}