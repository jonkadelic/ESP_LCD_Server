using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Linq;
using System.Threading;

namespace ESP_LCD_Server
{
    public static class WebInterface
    {
        // Variables
        private static HttpListener listener = new HttpListener();
        private static Thread listenerThread;
        private const string baseUrl = "http://+:80";

        // Properties
        public static bool IsRunning { get; private set; }
        public static List<IWebInterfaceMember> Members { get; } = new List<IWebInterfaceMember>();

        // Functions
        public static void AddMember(IWebInterfaceMember member)
        {
            if (Members.Select((x) => x.Endpoint).Contains(member.Endpoint))
            {
                throw new Exception("Endpoint is already in use!");
            }
            else if (IsRunning)
            {
                throw new Exception("Web Interface is already running!");
            }
            else
            {
                Members.Add(member);
                listener.Prefixes.Add($"{baseUrl}/{member.Endpoint}/");
            }
        }

        public static void Run()
        {
            listener.Start();

            listenerThread = new Thread(Task_Listen);
            listenerThread.Start();
            IsRunning = true;
        }

        private static void Task_Listen()
        {
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                string endpoint = context.Request.Url.Segments[1].Replace("/", "");
                IWebInterfaceMember validMember = null;
                foreach (IWebInterfaceMember member in Members)
                {
                    if (endpoint == member.Endpoint)
                    {
                        validMember = member;
                        break;
                    }
                }
                if (validMember == null)
                {
                    // No appropriate member to respond with
                    context.Response.StatusCode = 404;
                    context.Response.ContentLength64 = 0;
                }
                else
                {
                    // Found appropriate member
                    byte[] output = validMember.GetResponseBody(context.Request);
                    context.Response.StatusCode = 200;
                    context.Response.ContentLength64 = output.Length;
                    try
                    {
                        context.Response.OutputStream.Write(output, 0, output.Length);
                    }
                    catch (Exception) { }

                }
                context.Response.Close();
            }
        }
    }
}
