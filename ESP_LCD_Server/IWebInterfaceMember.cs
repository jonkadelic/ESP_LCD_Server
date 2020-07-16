using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ESP_LCD_Server
{
    public interface IWebInterfaceMember
    {
        // Properties
        public string Name { get; }
        public string Endpoint { get; }

        // Functions
        public byte[] GetResponseBody(HttpListenerRequest request);
    }
}
