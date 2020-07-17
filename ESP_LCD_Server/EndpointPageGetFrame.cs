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

        public byte[] GetResponseBody(string request)
        {
            return PageComposer.GetDisplayData();
        }
    }
}
