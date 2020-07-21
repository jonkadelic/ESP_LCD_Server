using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using OpenWeatherAPI;

namespace ESP_LCD_Server.Widgets
{
    public class Weather : BaseWidget
    {
        private readonly API weatherApi;
        private Query cachedQuery = null;
        private const int updatePeriodMs = 30000000;
        private Image weatherIcon = null;
        private readonly Font temperatureFont = new Font(FontFamily.GenericSansSerif, 30);
        private readonly Font medFont = new Font(FontFamily.GenericSansSerif, 12);
        private readonly Font smallFont = new Font(FontFamily.GenericSansSerif, 8);
        private readonly Font unitsFont = new Font(FontFamily.GenericSansSerif, 15);
        private readonly Color highestTemperatureColour = Color.FromArgb(255, 0, 0);
        private readonly Color lowestTemperatureColour = Color.FromArgb(0, 0, 255);
        private const int highestTemperature = 30;
        private const int lowestTemperature = 0;

        public override string Name => "Weather";

        public Weather()
        {
            weatherApi = new API(Secrets.WeatherToken);
            UpdateTask();
        }

        public override Bitmap RenderFrame()
        {
            Bitmap Frame = new Bitmap(FrameSize.Width, FrameSize.Height);
            if (cachedQuery == null || weatherIcon == null) return Frame;
            Graphics g = Graphics.FromImage(Frame);
            g.FillRectangle(Brushes.Black, new Rectangle(Point.Empty, Frame.Size));
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            string temperature = $"{(int)cachedQuery.Main.Temperature.CelsiusCurrent}";
            SizeF temperatureStringMeasurement = g.MeasureString(temperature, temperatureFont);
            Brush temperatureBrush = new SolidBrush(GetTemperatureColour((int)cachedQuery.Main.Temperature.CelsiusCurrent));
            g.DrawString(cachedQuery.Name, smallFont, Brushes.White, 3, 1);
            g.DrawString(temperature, temperatureFont, temperatureBrush, -6, 12);
            g.DrawString("°C", unitsFont, temperatureBrush, temperatureStringMeasurement.Width - 15, 14);
            g.DrawImage(weatherIcon, 80, 12, 40, 40);
            g.DrawString(cachedQuery.Weathers[0].Description, medFont, Brushes.White, new Rectangle(0, 54, FrameSize.Width, 16), new StringFormat() { Alignment = StringAlignment.Center });
            g.DrawString($"High:\t{(int)cachedQuery.Main.Temperature.CelsiusMaximum}°C\nLow:\t{(int)cachedQuery.Main.Temperature.CelsiusMinimum}°C\nSunrise:\t{cachedQuery.Sys.Sunrise.ToShortTimeString()}\nSunset:\t{cachedQuery.Sys.Sunset.ToShortTimeString()}\nWind:\t{(int)(cachedQuery.Wind.SpeedFeetPerSecond / 1.467)}mph", smallFont, Brushes.White, 3, 85);
            return Frame;
        }

        public override Task HandleActionAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates weather info at regular intervals.
        /// </summary>
        private async void UpdateTask()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        cachedQuery = weatherApi.Query("Colchester");

                        using WebClient webClient = new WebClient();
                        byte[] data = webClient.DownloadData($"http://openweathermap.org/img/wn/{cachedQuery.Weathers[0].Icon}@2x.png");
                        using MemoryStream ms = new MemoryStream(data);
                        weatherIcon = Image.FromStream(ms);
                    }
                    catch (Exception) { }
                    Thread.Sleep(updatePeriodMs);
                }
            });
        }

        /// <summary>
        /// Fetches a colour representing a temperature.
        /// </summary>
        /// <param name="temperature">Current temperature in degrees Celsius.</param>
        /// <returns>Colour representing the given temperature.</returns>
        private Color GetTemperatureColour(int temperature)
        {
            int temperatureRange = highestTemperature - lowestTemperature;
            int adjustedTemperature = temperature - lowestTemperature;
            float percentageTemperature = (float)adjustedTemperature / temperatureRange;
            int redRange = highestTemperatureColour.R - lowestTemperatureColour.R;
            int greenRange = highestTemperatureColour.G - lowestTemperatureColour.G;
            int blueRange = highestTemperatureColour.B - lowestTemperatureColour.B;
            return Color.FromArgb((int)(redRange * percentageTemperature) + lowestTemperatureColour.R,
                                  (int)(greenRange * percentageTemperature) + lowestTemperatureColour.G,
                                  (int)(blueRange * percentageTemperature) + lowestTemperatureColour.B);
        }
    }
}
