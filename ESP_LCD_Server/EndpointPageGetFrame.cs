using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text;

namespace ESP_LCD_Server
{
    public class EndpointPageGetFrame : IWebInterfaceEndpoint
    {
        public string Name => "Page Get Frame";

        public string Endpoint => "page_get_frame";

        public byte[] GetResponseBody(HttpListenerRequest request)
        {
            byte[] data = BitmapToRGBBytes(PageManager.CurrentPage.Frame);
            PageManager.CurrentPage.ReleaseFrame();
            return data;
        }

        private byte[] BitmapToRGBBytes(Bitmap bitmap)
        {
            byte[] data = new byte[bitmap.Width * bitmap.Height * 3];
            int i = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    data[i++] = color.R;
                    data[i++] = color.G;
                    data[i++] = color.B;
                }
            }

            return data;
        }
    }
}
