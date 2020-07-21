using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;
using ESP_LCD_Server.Endpoints;

namespace ESP_LCD_Server
{
    public static class UdpInterface
    {
        private const int PORT = 4567;
        private const int UDP_PACKET_SIZE = 1450; // max = 1472

        /// <summary>
        /// Endpoints in use by the UDP interface.
        /// </summary>
        public static List<IEndpoint> Endpoints { get; } = new List<IEndpoint>();

        /// <summary>
        /// Registers an endpoint with the UDP interface.
        /// </summary>
        /// <param name="endpoint">Endpoint to register.</param>
        public static void AddEndpoint(IEndpoint endpoint)
        {
            if (Endpoints.Select((x) => x.Endpoint).Contains(endpoint.Endpoint))
            {
                throw new Exception("Endpoint is already in use!");
            }
            else
            {
                Endpoints.Add(endpoint);
            }
        }

        /// <summary>
        /// Starts the UDP interface running.
        /// </summary>
        public static void Run()
        {
            new Thread(Task_Listen).Start();
        }

        /// <summary>
        /// Listener task that waits for incoming UDP traffic, then responds using a given endpoint handler.
        /// </summary>
        private static void Task_Listen()
        {
            byte[] dataBuffer;
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, PORT);
            UdpClient client = new UdpClient(ipep);

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                dataBuffer = client.Receive(ref sender);
                string targetEndpoint = Encoding.ASCII.GetString(dataBuffer);
                Debug.WriteLine($"Received UDP endpoint request {targetEndpoint}.");
                DateTime startTime = DateTime.Now;
                IEndpoint validEndpoint = null;
                foreach (IEndpoint endpoint in Endpoints)
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
                    byte[] response = Encoding.ASCII.GetBytes("Failure");
                    client.Send(response, response.Length, sender);
                }
                else
                {
                    // Found appropriate endpoint
                    DateTime responseStartTime = DateTime.Now;
                    byte[] output = validEndpoint.GetResponseBody(targetEndpoint);
                    TimeSpan responseTime = DateTime.Now - responseStartTime;

                    Debug.WriteLine($"Replying with {output.Length} bytes of data to {sender.Address}:{sender.Port}.");

                    if (output.Length <= UDP_PACKET_SIZE)
                    {
                        client.Send(output, output.Length, sender);
                    }
                    else
                    {
                        int i;
                        for (i = 0; i < output.Length / UDP_PACKET_SIZE; i++)
                        {
                            client.Send(output[(i * UDP_PACKET_SIZE)..], UDP_PACKET_SIZE, sender);
                        }
                        client.Send(output[(i * UDP_PACKET_SIZE)..], output.Length - (i * UDP_PACKET_SIZE), sender);
                    }

                    Debug.WriteLine($"Took {(int)(DateTime.Now - startTime).TotalMilliseconds}ms to respond to \"{targetEndpoint}\" ({(int)responseTime.TotalMilliseconds}ms to generate body).");
                }
            }
        }
    }
}
