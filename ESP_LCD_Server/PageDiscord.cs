using Discord.WebSocket;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ESP_LCD_Server
{
    public class PageDiscord : AbstractPage
    {
        private DiscordSocketClient client;
        private SocketMessage lastMessage = null;
        private readonly Font headerFont = new Font(FontFamily.GenericSansSerif, 10);
        private readonly StringFormat headerFormat = new StringFormat() { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        private readonly Font nameTimeFont = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
        private readonly Font messageFont = new Font(FontFamily.GenericSansSerif, 8);
        private Image authorAvatar = null;
        private Image attachment = null;

        public override string Name => "Discord";
        public override string Endpoint => "page_discord";


        public PageDiscord()
        {
            client = new DiscordSocketClient();
            UpdateTask();
        }

        public async override Task RenderFrameAsync()
        {
            frame = new Bitmap(frameWidth, frameHeight);
            await Task.Run(() =>
            {
                Graphics g = Graphics.FromImage(frame);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                if (lastMessage == null)
                {
                    g.DrawString("No messages yet :(", headerFont, Brushes.White, new Rectangle(0, 0, frameWidth, frameHeight), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    return;
                }

                g.DrawString(lastMessage.Channel.Name, headerFont, Brushes.White, new Rectangle(38, 0, frameWidth - 38, 20), headerFormat);
                g.DrawString($"{lastMessage.Author.Username} - {lastMessage.CreatedAt.DateTime.ToShortTimeString()}", nameTimeFont, Brushes.White, 38, 20);
                int attachmentHeight = 0;
                if (attachment != null)
                {
                    attachmentHeight = (int)(((float)frameWidth / attachment.Width) * attachment.Height);
                    g.DrawImage(attachment, new Rectangle(0, 40, frameWidth, attachmentHeight));
                }
                g.DrawString(lastMessage.Content, messageFont, Brushes.White, new Rectangle(0, 40 + attachmentHeight, frameWidth, frameHeight - 40));
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(new Rectangle(1, 1, 36, 36));
                g.SetClip(path);
                if (authorAvatar != null)
                    g.DrawImage(authorAvatar, new Rectangle(1, 1, 36, 36));
            });
        }

        protected async override Task HandleActionAsync()
        {
            return;
        }

        private async void UpdateTask()
        {
            client.MessageReceived += Client_MessageReceived;
            client.Log += Client_Log;
            await client.LoginAsync(0, Secrets.DiscordToken);
            await client.StartAsync();
        }

        private Task Client_Log(Discord.LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        private Task Client_MessageReceived(SocketMessage arg)
        {
            if ((arg.Channel is SocketDMChannel || arg.Channel is SocketGroupChannel) && (arg.Author.Id != client.CurrentUser.Id))
            {
                lastMessage = arg;

                using WebClient webClient = new WebClient();
                string url;
                if ((url = arg.Author.GetAvatarUrl()) == null)
                {
                    url = arg.Author.GetDefaultAvatarUrl();
                }
                byte[] data = webClient.DownloadData(url);
                using MemoryStream ms = new MemoryStream(data);
                authorAvatar = Image.FromStream(ms);

                if (arg.Attachments.Count > 0)
                {
                    data = webClient.DownloadData(arg.Attachments.First().Url);
                    using MemoryStream ms2 = new MemoryStream(data);
                    attachment = Image.FromStream(ms2);
                }
                else
                {
                    attachment = null;
                }
            }
            return Task.CompletedTask;
        }
    }
}
