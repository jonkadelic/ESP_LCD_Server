using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace ESP_LCD_Server
{
    public abstract class AbstractPage : IWebInterfaceMember
    {
        protected Image frame;
        protected const int REGION_HEIGHT = 160;
        protected const int BYTES_PER_PIXEL = 3;
        protected const int frameWidth = 128;
        protected const int frameHeight = 160;
        protected bool needsFullRefresh = false;

        public event NotifyEventHandler Notify;
        public delegate void NotifyEventHandler(AbstractPage sender);

        public abstract string Name { get; }
        public abstract string Endpoint { get; }
        public abstract int NotifyDurationMs { get; }

        public byte[] GetResponseBody(HttpListenerRequest request)
        {
            Console.WriteLine($"Request from {request.Url}");
            string[] queryParts = request.Url.Query[1..].Split("&");
            List<KeyValuePair<string, string>> kvps = new List<KeyValuePair<string, string>>();
            foreach (string qp in queryParts)
            {
                string[] parts = qp.Split("=");
                if (parts.Length == 1)
                {
                    kvps.Add(new KeyValuePair<string, string>(parts[0], ""));
                }
                else
                {
                    kvps.Add(new KeyValuePair<string, string>(parts[0], parts[1]));
                }
            }

            switch (kvps[0].Key)
            {
                case "region":
                    try
                    {
                        if (needsFullRefresh)
                        {
                            needsFullRefresh = false;
                            Console.WriteLine("Instructing full refresh.");
                            return Encoding.ASCII.GetBytes("Needs Refresh");
                        }
                        else
                        {
                            return GetFrameRegion(int.Parse(kvps[0].Value));
                        }
                    }
                    catch (FormatException)
                    {
                        return new byte[0];
                    }
                case "action":
                    HandleActionAsync();
                    return Encoding.UTF8.GetBytes("Success");
                default:
                    return new byte[0];
            }
        }

        protected byte[] GetFrameRegion(int line)
        {
            if (frame.Width != frameWidth || frame.Height != frameHeight)
            {
                throw new Exception("Frame was the wrong size!");
            }
            else if (line > frameHeight / REGION_HEIGHT)
            {
                return new byte[0];
            }

            byte[] data = new byte[frameWidth * REGION_HEIGHT * BYTES_PER_PIXEL];
            int i = 0;

            using (Bitmap bmp = new Bitmap(frame))
            {
                for (int y = line * REGION_HEIGHT; y < line * REGION_HEIGHT + REGION_HEIGHT; y++)
                {
                    for (int x = 0; x < frameWidth; x++)
                    {
                        Color color = bmp.GetPixel(x, y);
                        data[i++] = color.R;
                        data[i++] = color.G;
                        data[i++] = color.B;
                    }
                }
            }

            return data;
        }

        protected void OnNotify()
        {
            NotifyEventHandler handler = Notify;
            handler?.Invoke(this);
        }

        public abstract Task RenderFrameAsync();

        protected abstract Task HandleActionAsync();
    }
}
