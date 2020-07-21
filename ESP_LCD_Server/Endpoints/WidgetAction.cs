using System.Text;

namespace ESP_LCD_Server.Endpoints
{
    public class WidgetAction : IEndpoint
    {
        public string Endpoint => "widget_action";

        public byte[] GetResponseBody(string request)
        {
            WidgetManager.Action();

            return Encoding.ASCII.GetBytes("Success");
        }
    }
}
