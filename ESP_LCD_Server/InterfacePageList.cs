using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ESP_LCD_Server
{
    public class InterfacePageList : IWebInterfaceMember
    {
        public string Name => "Page List";

        public string Endpoint => "list_pages";

        public byte[] GetResponseBody(HttpListenerRequest request)
        {
            StringBuilder output = new StringBuilder();

            for (int i = 0; i < WebInterface.Members.Count - 1; i++)
            {
                if (WebInterface.Members[i] == this) continue;
                output.Append(WebInterface.Members[i].Endpoint + "\n");
            }
            output.Append(WebInterface.Members.Last().Endpoint + "\0");

            return Encoding.ASCII.GetBytes(output.ToString());
        }
    }
}
