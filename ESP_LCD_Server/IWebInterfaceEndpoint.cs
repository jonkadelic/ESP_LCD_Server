using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ESP_LCD_Server
{
    public interface IWebInterfaceEndpoint
    {
        // Properties
        public string Name { get; }
        public string Endpoint { get; }

        // Functions
        public byte[] GetResponseBody(string request);
    }
}
