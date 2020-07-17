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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ESP_LCD_Server
{
    public class PageDiscord : AbstractNotifyingPage
    {
        private DiscordSocketClient client;
        private SocketMessage lastMessage = null;
        private string lastMessageContent = null;
        private readonly Font headerFont = new Font("Segoe UI Emoji", 10);
        private readonly StringFormat headerFormat = new StringFormat() { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        private readonly Font nameTimeFont = new Font("Segoe UI Emoji", 8, FontStyle.Bold);
        private readonly Font messageFont = new Font("Segoe UI Emoji", 8);
        private Image authorAvatar = null;
        private Image attachment = null;

        public override string Name => "Discord";
        public override int NotifyDurationMs => 5000;


        public PageDiscord()
        {
            client = new DiscordSocketClient();
            UpdateTask();
        }

        public override Bitmap RenderFrame()
        {
            Bitmap Frame = new Bitmap(FRAME_WIDTH, FRAME_HEIGHT);
            using Graphics g = Graphics.FromImage(Frame);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            if (lastMessage == null)
            {
                g.DrawString("No messages yet :(", headerFont, Brushes.White, new Rectangle(0, 0, FRAME_WIDTH, FRAME_HEIGHT), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                return Frame;
            }

            g.DrawString(lastMessage.Channel.Name, headerFont, Brushes.White, new Rectangle(38, 0, FRAME_WIDTH - 38, 20), headerFormat);
            g.DrawString($"{lastMessage.Author.Username} - {lastMessage.CreatedAt.LocalDateTime.ToShortTimeString()}", nameTimeFont, Brushes.White, 38, 20);
            int attachmentHeight = 0;
            if (attachment != null)
            {
                attachmentHeight = (int)(((float)FRAME_WIDTH / attachment.Width) * attachment.Height);
                g.DrawImage(attachment, new Rectangle(0, 40, FRAME_WIDTH, attachmentHeight));
            }
            g.DrawString(lastMessageContent, messageFont, Brushes.White, new Rectangle(0, 40 + attachmentHeight, FRAME_WIDTH, FRAME_HEIGHT - 40));
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(new Rectangle(1, 1, 36, 36));
            g.SetClip(path);
            if (authorAvatar != null)
                g.DrawImage(authorAvatar, new Rectangle(1, 1, 36, 36));

            return Frame;
        }

        public override Bitmap RenderNotifyFrame()
        {
            Bitmap Frame = new Bitmap(FRAME_WIDTH, FRAME_HEIGHT / 3);
            using Graphics g = Graphics.FromImage(Frame);
            g.FillRectangle(Brushes.Black, new Rectangle(Point.Empty, Frame.Size));
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString($"{lastMessage.Author.Username}", nameTimeFont, Brushes.White, 40, 0);

            g.DrawString(lastMessageContent, messageFont, Brushes.White, new Rectangle(40, 12, Frame.Width - 40, Frame.Height - 12));

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
            if ((arg.Channel is SocketDMChannel || arg.Channel is SocketGroupChannel || arg.MentionedUsers.Select(x => x.Id).Contains(client.CurrentUser.Id) == true) )//&& (arg.Author.Id != client.CurrentUser.Id))
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

                OnNotify();
            }
            return Task.CompletedTask;
        }
    }
}
