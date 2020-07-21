using Discord.WebSocket;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ESP_LCD_Server.Widgets
{
    public class DiscordMessages : BaseNotifyingWidget
    {
        private DiscordSocketClient client;
        private SocketMessage lastMessage = default;
        private string lastMessageContent = default;
        private string lastMessageNickname = default;
        private readonly Font headerFont = new Font("Segoe UI Emoji", 10);
        private readonly StringFormat headerFormat = new StringFormat() { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        private readonly Font nameTimeFont = new Font("Segoe UI Emoji", 8, FontStyle.Bold);
        private readonly Font messageFont = new Font("Segoe UI Emoji", 8);
        private Bitmap authorAvatar = null;
        private Bitmap attachment = null;

        public override string Name => "Discord";
        public override int NotifyDurationMs => 5000;


        public DiscordMessages()
        {
            client = new DiscordSocketClient();

            client.MessageReceived += Client_MessageReceived;
            client.Log += Client_Log;
            client.LoginAsync(0, Secrets.DiscordToken);
            client.StartAsync();
        }

        public override Bitmap RenderFrame()
        {
            Bitmap Frame = new Bitmap(FrameSize.Width, FrameSize.Height);
            using Graphics g = Graphics.FromImage(Frame);
            g.FillRectangle(Brushes.Black, new Rectangle(Point.Empty, FrameSize));
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            if (lastMessage == null)
            {
                g.DrawString("No messages yet :(", headerFont, Brushes.White, new Rectangle(Point.Empty, FrameSize), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                return Frame;
            }

            g.DrawString(lastMessage.Channel.Name, headerFont, Brushes.White, new Rectangle(38, 0, FrameSize.Width - 38, 20), headerFormat);
            g.DrawString($"{lastMessage.Author.Username} - {lastMessage.CreatedAt.LocalDateTime.ToShortTimeString()}", nameTimeFont, Brushes.White, 38, 20);
            int attachmentHeight = 0;
            if (attachment != null)
            {
                attachmentHeight = (int)(((float)FrameSize.Width / attachment.Width) * attachment.Height);
                g.DrawImage(attachment, new Rectangle(0, 40, FrameSize.Width, attachmentHeight));
            }
            g.DrawString(lastMessageContent, messageFont, Brushes.White, new Rectangle(0, 40 + attachmentHeight, FrameSize.Width, FrameSize.Height - 40));
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(new Rectangle(1, 1, 36, 36));
            g.SetClip(path);
            if (authorAvatar != null)
                g.DrawImage(authorAvatar, new Rectangle(1, 1, 36, 36));

            return Frame;
        }

        public override Task HandleActionAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Log Discord.NET library data to the console.
        /// </summary>
        /// <param name="arg">Log message to write.</param>
        /// <returns>Task status.</returns>
        private Task Client_Log(Discord.LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle incoming messages from Discord.NET.
        /// </summary>
        /// <param name="arg">SocketMessage to handle.</param>
        /// <returns>Task status.</returns>
        private Task Client_MessageReceived(SocketMessage arg)
        {
            if ((arg.Channel is SocketDMChannel || arg.Channel is SocketGroupChannel || arg.MentionedUsers.Select(x => x.Id).Contains(client.CurrentUser.Id) == true) && (arg.Author.Id != client.CurrentUser.Id))
            {
                lastMessage = arg;

                Regex regex = new Regex(@"<@!(\d+)>");
                lastMessageContent = regex.Replace(lastMessage.Content, (x) =>
                {
                    ulong userId = ulong.Parse(x.Groups[1].Value);
                    string usernameClean;
                    if (arg.Channel is SocketGuildChannel)
                    {
                        usernameClean = (arg.Channel as SocketGuildChannel).GetUser(userId).Nickname;
                    }
                    else
                    {
                        usernameClean = client.GetUser(userId).Username.Split("#")[0];
                    }
                    return $"[@{usernameClean}]";
                });

                using WebClient webClient = new WebClient();
                string url;
                if ((url = arg.Author.GetAvatarUrl()) == null)
                {
                    url = arg.Author.GetDefaultAvatarUrl();
                }
                byte[] data = webClient.DownloadData(url);
                using MemoryStream ms = new MemoryStream(data);
                authorAvatar = (Bitmap)Image.FromStream(ms);

                if (arg.Attachments.Count > 0)
                {
                    data = webClient.DownloadData(arg.Attachments.First().Url);
                    using MemoryStream ms2 = new MemoryStream(data);
                    attachment = (Bitmap)Image.FromStream(ms2);
                }
                else
                {
                    attachment = null;
                }

                Notification notif = new Notification(lastMessageContent, this, authorAvatar, Notification.ICON_STYLE.ROUND, NotifyDurationMs, lastMessage.Author.Username);

                OnNotify(notif);
            }
            return Task.CompletedTask;
        }
    }
}
