using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SpotifyAPI.Web;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.IO;
using SuperfastBlur;

namespace ESP_LCD_Server
{
    public class PageSpotify : AbstractPage
    {
        private readonly string clientToken = Secrets.SpotifyClientToken;
        private string accessToken = null;
        private readonly string refreshToken = Secrets.SpotifyRefreshToken;
        private SpotifyClient client = null;
        private DateTime nextAccessTokenRefresh;
        private CurrentlyPlaying cachedCurrentlyPlaying = null;
        private const int updatePeriodMs = 5000;
        private System.Drawing.Image artImage;
        private Bitmap artImageBlurred;
        private Font smallFont = new Font(FontFamily.GenericSansSerif, 7);
        private Font bigFont = new Font(FontFamily.GenericSansSerif, 10);
        private StringFormat stringFormat = new StringFormat() { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        private StringFormat pauseFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        private bool renderArtBig = false;
        private int artSmallDims = 100;
        private int artLargeDims = 128;


        public override string Name => "Spotify";
        public override string Endpoint => "page_spotify";
        public override int NotifyDurationMs => 1000;

        public PageSpotify()
        {
            UpdateTask();
        }

        protected override async Task HandleActionAsync()
        {
            await Task.Run(() =>
            {
                needsFullRefresh = true;
                renderArtBig = !renderArtBig;
            });
        }

        public override async Task RenderFrameAsync()
        {
            frame = new Bitmap(frameWidth, frameHeight);
            await Task.Run(() =>
            {
                string songName, songArtist, songAlbum;
                Graphics g = Graphics.FromImage(frame);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

                if (cachedCurrentlyPlaying == null)
                {
                    g.DrawString("Nothing is playing on Spotify :(", bigFont, Brushes.White, new Rectangle(0, 0, frameWidth, frameHeight), pauseFormat);
                    return;
                }

                if (renderArtBig)
                {
                    if (artImage != null)
                    {
                        g.DrawImage(artImage, new Rectangle(0, 16, artLargeDims, artLargeDims));
                    }
                    if (cachedCurrentlyPlaying.IsPlaying == false)
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(127, Color.Black)), new Rectangle(0, 16, artLargeDims, artLargeDims));
                        g.DrawString("| |", bigFont, Brushes.White, new Rectangle(0, 16, artLargeDims, artLargeDims), pauseFormat);
                        g.DrawString("| |", bigFont, Brushes.White, new Rectangle(1, 16, artLargeDims, artLargeDims), pauseFormat);
                    }
                }
                else
                {
                    if (artImageBlurred != null)
                    {
                        g.DrawImage(artImageBlurred, new Rectangle(-16, 0, frameHeight, frameHeight));
                    }
                    g.FillRectangle(new SolidBrush(Color.FromArgb(127, Color.Black)), new Rectangle(0, 0, frameWidth, frameHeight));

                    if (artImage != null)
                    {
                        g.DrawImage(artImage, new Rectangle(14, 50, artSmallDims, artSmallDims));
                    }
                    if (cachedCurrentlyPlaying.IsPlaying == false)
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(127, Color.Black)), new Rectangle(14, 50, artSmallDims, artSmallDims));
                        g.DrawString("| |", bigFont, Brushes.White, new Rectangle(14, 50, artSmallDims, artSmallDims), pauseFormat);
                        g.DrawString("| |", bigFont, Brushes.White, new Rectangle(15, 50, artSmallDims, artSmallDims), pauseFormat);
                    }
                    if (cachedCurrentlyPlaying.Item.Type == ItemType.Track)
                    {
                        FullTrack track = cachedCurrentlyPlaying.Item as FullTrack;
                        songName = track.Name;
                        songArtist = track.Artists[0].Name;
                        songAlbum = track.Album.Name;
                    }
                    else
                    {
                        FullEpisode episode = cachedCurrentlyPlaying.Item as FullEpisode;
                        songName = episode.Name;
                        songArtist = episode.Show.Publisher;
                        songAlbum = episode.Show.Name;
                    }
                    g.DrawString($"{songName}", bigFont, Brushes.Black, new Rectangle(1, 1, frameWidth, 16), stringFormat);
                    g.DrawString($"{songName}", bigFont, Brushes.White, new Rectangle(0, 0, frameWidth, 16), stringFormat);
                    g.DrawString($"{songAlbum}\n{songArtist}", smallFont, Brushes.Black, new Rectangle(1, 17, frameWidth, 24), stringFormat);
                    g.DrawString($"{songAlbum}\n{songArtist}", smallFont, Brushes.White, new Rectangle(0, 16, frameWidth, 24), stringFormat);
                }
            });
        }

        private async void UpdateTask()
        {
            while (true)
            {
                await UpdateCachedCurrentlyPlaying();
                Thread.Sleep(updatePeriodMs);
            }
        }

        private async Task UpdateCachedCurrentlyPlaying()
        {
            if (accessToken == null || client == null || DateTime.Now > nextAccessTokenRefresh)
            {
                await RefreshAccessToken();
                client = new SpotifyClient(accessToken);
            }
            CurrentlyPlaying newCurrentlyPlaying = await client.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.All));
            if (newCurrentlyPlaying == null)
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
            needsFullRefresh = true;

            string artUrl = (cachedCurrentlyPlaying.Item.Type == ItemType.Track) ? ((FullTrack)cachedCurrentlyPlaying.Item).Album.Images[0].Url : ((FullEpisode)cachedCurrentlyPlaying.Item).Show.Images[0].Url;

            using WebClient webClient = new WebClient();
            byte[] data = webClient.DownloadData(artUrl);
            using MemoryStream ms = new MemoryStream(data);
            artImage = System.Drawing.Image.FromStream(ms);
            artImageBlurred = new GaussianBlur((Bitmap)artImage).Process(20);

            OnNotify();
        }

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
