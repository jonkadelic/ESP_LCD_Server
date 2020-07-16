using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ESP_LCD_Server
{
    public class EndpointPageAction : IWebInterfaceEndpoint
    {
        public string Name => "Page Action";

        public string Endpoint => "page_action";

        public byte[] GetResponseBody(HttpListenerRequest request)
        {
            PageManager.Action();

            return Encoding.ASCII.GetBytes("Success");
        }
    }
}
