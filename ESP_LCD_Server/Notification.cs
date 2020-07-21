using ESP_LCD_Server.Widgets;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ESP_LCD_Server
{
    public class Notification
    {
        private const int DEFAULT_DISPLAY_TIME_MS = 3000;
        private static readonly Font headerFont = new Font("Segoe UI Emoji", 8, FontStyle.Bold);
        private static readonly Font bodyFont = new Font("Segoe UI Emoji", 8);

        /// <summary>
        /// Represents the method used to draw the icon.
        /// </summary>
        public enum ICON_STYLE
        {
            /// <summary>
            /// Icon is drawn as a square.
            /// </summary>
            SQUARE,
            /// <summary>
            /// Icon is drawn as a circle.
            /// </summary>
            ROUND
        }

        /// <summary>
        /// Widget that sent this notification.
        /// </summary>
        public BaseNotifyingWidget Origin { get; }
        /// <summary>
        /// Icon that represents this widget.
        /// </summary>
        public Bitmap Icon { get; }
        /// <summary>
        /// Method used to draw the icon.
        /// </summary>
        public ICON_STYLE IconStyle { get; }
        /// <summary>
        /// Duration to display the notification for in milliseconds.
        /// </summary>
        public int DisplayTime { get; }
        /// <summary>
        /// Header text to display at the top of the notification.
        /// </summary>
        public string Header { get; }
        /// <summary>
        /// Main content of the notification.
        /// </summary>
        public string Body { get; }

        /// <summary>
        /// Creates a new Notification.
        /// </summary>
        /// <param name="body">Main content of the notification.</param>
        /// <param name="origin">Widget that sent this notification.</param>
        /// <param name="icon">Icon that represents this widget.</param>
        /// <param name="iconStyle">Method used to draw the icon.</param>
        /// <param name="displayTime">Duration to display the notification for in milliseconds.</param>
        /// <param name="header">Header text to display at the top of the notification.</param>
        public Notification(string body, BaseNotifyingWidget origin = default, Bitmap icon = default, ICON_STYLE iconStyle = default, int displayTime = DEFAULT_DISPLAY_TIME_MS, string header = default)
        {
            Body = body;
            Origin = origin;
            Icon = icon;
            IconStyle = iconStyle;
            DisplayTime = displayTime;
            Header = header;
        }

        public static Bitmap RenderNotification(Notification notification)
        {
            using Graphics tempG = Graphics.FromImage(new Bitmap(1, 1));
            int stringHeight = (int)tempG.MeasureString(notification.Body, bodyFont, BaseWidget.FrameSize.Width - 40).Height;
            if (stringHeight > BaseWidget.FrameSize.Height - 16) stringHeight = BaseWidget.FrameSize.Height - 16;
            else if (stringHeight + 16 < 38) stringHeight = 22;
            Bitmap Frame = new Bitmap(BaseWidget.FrameSize.Height, stringHeight + 16);
            using Graphics g = Graphics.FromImage(Frame);
            g.FillRectangle(Brushes.Black, new Rectangle(Point.Empty, Frame.Size));
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString(notification.Header, headerFont, Brushes.White, 40, 0);

            g.DrawString(notification.Body, bodyFont, Brushes.White, new Rectangle(40, 12, Frame.Width - 40, Frame.Height - 12));

            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(new Rectangle(1, 1, 36, 36));
            if (notification.IconStyle == ICON_STYLE.ROUND)
                g.SetClip(path);
            if (notification.Icon != null)
                g.DrawImage(notification.Icon, new Rectangle(1, 1, 36, 36));

            return Frame;
        }
    }
}
