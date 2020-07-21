using System.Text;

namespace ESP_LCD_Server.Endpoints
{
    public class WidgetNext : IEndpoint
    {
        public string Endpoint => "widget_next";

        public byte[] GetResponseBody(string request)
        {
            WidgetManager.NextWidget();

            return Encoding.ASCII.GetBytes("Success");
        }
    }
}
