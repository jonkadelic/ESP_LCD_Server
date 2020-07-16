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
        public static List<IWebInterfaceEndpoint> Endpoints { get; } = new List<IWebInterfaceEndpoint>();

        // Functions
        public static void AddEndpoint(IWebInterfaceEndpoint endpoint)
        {
            if (Endpoints.Select((x) => x.Endpoint).Contains(endpoint.Endpoint))
            {
                throw new Exception("Endpoint is already in use!");
            }
            else if (IsRunning)
            {
                throw new Exception("Web Interface is already running!");
            }
            else
            {
                Endpoints.Add(endpoint);
                listener.Prefixes.Add($"{baseUrl}/{endpoint.Endpoint}/");
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
                string targetEndpoint = context.Request.Url.Segments[1].Replace("/", "");
                IWebInterfaceEndpoint validEndpoint = null;
                foreach (IWebInterfaceEndpoint endpoint in Endpoints)
                {
                    if (targetEndpoint == endpoint.Endpoint)
                    {
                        validEndpoint = endpoint;
                        break;
                    }
                }
                if (validEndpoint == null)
                {
                    // No appropriate endpoint to respond with
                    context.Response.StatusCode = 404;
                    context.Response.ContentLength64 = 0;
                }
                else
                {
                    Console.WriteLine($"Valid request from {context.Request.Url}.");
                    // Found appropriate endpoint
                    byte[] output = validEndpoint.GetResponseBody(context.Request);
                    context.Response.StatusCode = 200;
                    context.Response.ContentLength64 = output.Length;
                    try
                    {
                        context.Response.OutputStream.Write(output, 0, output.Length);
                        Console.WriteLine($"Wrote response for {context.Request.Url}.");
                    }
                    catch (Exception) { }

                }
                context.Response.Close();
            }
        }
    }
}
