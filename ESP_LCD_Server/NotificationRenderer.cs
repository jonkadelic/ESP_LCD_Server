using LCDWidget;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ESP_LCD_Server
{
    public class NotificationRenderer
    {
        private static readonly Font headerFont = new Font("Segoe UI Emoji", 8, FontStyle.Bold);
        private static readonly StringFormat headerFormat = new StringFormat(StringFormatFlags.NoWrap) { Trimming = StringTrimming.EllipsisCharacter };
        private static readonly Font bodyFont = new Font("Segoe UI Emoji", 8);

        private static Rectangle iconSpace = new Rectangle(Point.Empty, new Size(BaseWidget.FrameSize.Width / 3, BaseWidget.FrameSize.Width / 3));
        private static Rectangle iconRect = new Rectangle(iconSpace.Location + new Size(2, 2), iconSpace.Size - new Size(4, 4));

        public static Bitmap RenderNotification(Notification notification)
        {
            using Graphics tempG = Graphics.FromImage(new Bitmap(1, 1));
            int stringHeight = (int)tempG.MeasureString(notification.Body, bodyFont, BaseWidget.FrameSize.Width - iconSpace.Width).Height;
            if (stringHeight > BaseWidget.FrameSize.Height - 16) stringHeight = BaseWidget.FrameSize.Height - 16;
            else if (stringHeight + 16 < iconSpace.Height) stringHeight = iconSpace.Height;

            Bitmap Frame = new Bitmap(BaseWidget.FrameSize.Width, stringHeight + 16);
            using Graphics g = Graphics.FromImage(Frame);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            g.FillRectangle(Brushes.Black, new Rectangle(Point.Empty, Frame.Size));
            g.DrawString(notification.Header, headerFont, Brushes.White, new Rectangle(new Point(iconSpace.Right, 0), new Size(Frame.Width - iconSpace.Width, 16)), headerFormat);
            g.DrawString(notification.Body, bodyFont, Brushes.White, new Rectangle(iconSpace.Right, 12, Frame.Width - iconSpace.Width, Frame.Height - 12));

            if (notification.IconStyle == Notification.ICON_STYLE.ROUND)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(iconRect);
                g.SetClip(path);
            }
            if (notification.Icon != null)
                g.DrawImage(notification.Icon, iconRect);

            return Frame;
        }
    }
}
