using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ESP_LCD_Server
{
    public class EndpointPageLast : IWebInterfaceEndpoint
    {
        public string Name => "Last Page";

        public string Endpoint => "page_last";

        public byte[] GetResponseBody(string request)
        {
            PageManager.LastPage();

            return Encoding.ASCII.GetBytes("Success");
        }
    }
}
