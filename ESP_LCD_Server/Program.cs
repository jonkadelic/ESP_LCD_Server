namespace ESP_LCD_Server
{
    class Program
    {
        static Widgets.Clock widget_clock;
        static Widgets.Spotify widget_spotify;
        static Widgets.Weather widget_weather;
        static Widgets.DiscordMessages widget_discord;
        static Widgets.D20 widget_d20;

        static void Main()
        {
            widget_clock = new Widgets.Clock();
            widget_spotify = new Widgets.Spotify();
            widget_weather = new Widgets.Weather();
            widget_discord = new Widgets.DiscordMessages();
            widget_d20 = new Widgets.D20();

            WidgetManager.AddWidget(widget_clock);
            WidgetManager.AddWidget(widget_spotify);
            WidgetManager.AddWidget(widget_weather);
            WidgetManager.AddWidget(widget_discord);
            WidgetManager.AddWidget(widget_d20);

            Endpoints.WidgetAction endpoint_page_action = new Endpoints.WidgetAction();
            Endpoints.WidgetGetFrame endpoint_page_get_frame = new Endpoints.WidgetGetFrame();
            Endpoints.WidgetNext endpoint_page_next = new Endpoints.WidgetNext();
            Endpoints.WidgetLast endpoint_page_last = new Endpoints.WidgetLast();

            UdpInterface.AddEndpoint(endpoint_page_action);
            UdpInterface.AddEndpoint(endpoint_page_get_frame);
            UdpInterface.AddEndpoint(endpoint_page_next);
            UdpInterface.AddEndpoint(endpoint_page_last);

            UdpInterface.Run();
        }
    }
}
