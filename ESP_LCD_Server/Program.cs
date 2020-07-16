using System;
using System.Threading;
using System.Threading.Tasks;

namespace ESP_LCD_Server
{
    class Program
    {
        static void Main()
        {
            InterfacePageList interfacePageList = new InterfacePageList();
            PageSpotify page_spotify = new PageSpotify();
            PageWeather page_weather = new PageWeather();
            PageDiscord page_discord = new PageDiscord();

            WebInterface.AddMember(interfacePageList);
            WebInterface.AddMember(page_spotify);
            WebInterface.AddMember(page_weather);
            WebInterface.AddMember(page_discord);

            new Thread(RenderPages).Start();

            WebInterface.Run();
        }

        static async void RenderPages()
        {
            while (true)
            {
                foreach(IWebInterfaceMember member in WebInterface.Members)
                {
                    if (member is AbstractPage)
                    {
                        await (member as AbstractPage).RenderFrameAsync();
                    }
                }
                Thread.Sleep(250);
            }
        }
    }
}
