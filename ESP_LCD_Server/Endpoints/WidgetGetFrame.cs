namespace ESP_LCD_Server.Endpoints
{
    public class WidgetGetFrame : IEndpoint
    {
        public string Endpoint => "widget_get_frame";

        public byte[] GetResponseBody(string request)
        {
            return WidgetComposer.GetDisplayData();
        }
    }
}
