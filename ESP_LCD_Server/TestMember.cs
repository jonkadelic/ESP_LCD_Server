using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ESP_LCD_Server
{
    public class TestMember : IWebInterfaceEndpoint
    {
        // Properties
        public string Name { get; }
        public string Endpoint { get; }

        // Constructors
        public TestMember(string name, string endpoint)
        {
            Name = name;
            Endpoint = endpoint;
        }

        // Functions
        public byte[] GetResponseBody(HttpListenerRequest request)
        {
            return Encoding.UTF8.GetBytes("Success");
        }
    }
}
