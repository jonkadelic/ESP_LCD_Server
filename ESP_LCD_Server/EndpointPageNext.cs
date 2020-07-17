using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ESP_LCD_Server
{
    public class EndpointPageNext : IWebInterfaceEndpoint
    {
        public string Name => "Next Page";

        public string Endpoint => "page_next";

        public byte[] GetResponseBody(string request)
        {
            PageManager.NextPage();

            return Encoding.ASCII.GetBytes("Success");
        }
    }
}
