using LCDWidget;
using System;

namespace ESP_LCD_Server
{
    class Program
    {
        static void Main()
        {
            Logger.InitLogger(true);

            WidgetManager.LoadWidgets();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

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

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Logger.Log(ex.Message, sender.GetType());
            Logger.Log(ex.StackTrace, sender.GetType());
        }
    }
}
