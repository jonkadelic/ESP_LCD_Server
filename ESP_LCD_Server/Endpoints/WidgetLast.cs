using System.Text;

namespace ESP_LCD_Server.Endpoints
{
    public class WidgetLast : IEndpoint
    {
        public string Endpoint => "widget_last";

        public byte[] GetResponseBody(string request)
        {
            WidgetManager.LastWidget();

            return Encoding.ASCII.GetBytes("Success");
        }
    }
}
