using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESP_LCD_Server
{
    class Program
    {
        static void Main()
        {
            PageSpotify page_spotify = new PageSpotify();
            PageWeather page_weather = new PageWeather();
            PageDiscord page_discord = new PageDiscord();

            PageManager.AddPage(page_spotify);
            PageManager.AddPage(page_weather);
            PageManager.AddPage(page_discord);

            EndpointPageAction endpoint_page_action = new EndpointPageAction();
            EndpointPageGetFrame endpoint_page_get_frame = new EndpointPageGetFrame();
            EndpointPageNext endpoint_page_next = new EndpointPageNext();
            EndpointPageLast endpoint_page_last = new EndpointPageLast();

            WebInterface.AddEndpoint(endpoint_page_action);
            WebInterface.AddEndpoint(endpoint_page_get_frame);
            WebInterface.AddEndpoint(endpoint_page_next);
            WebInterface.AddEndpoint(endpoint_page_last);

            WebInterface.Run();
        }
    }
}
