using LCDWidget;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ESP_LCD_Server
{
    public static class WidgetComposer
    {
        private const int WIDGET_TRANSITION_DURATION_MS = 250;
        private const int NOTIFY_INOUT_ANIM_DURATION_MS = 250;
        private const int FOOTER_DURATION_MS = 2000;
        private const int FOOTER_INOUT_ANIM_DURATION_MS = 100;

        private static LCDWidget.Animators.LinearInOut notifyAnimator = new LCDWidget.Animators.LinearInOut(inDurationMs: NOTIFY_INOUT_ANIM_DURATION_MS,
                                                                                    outDurationMs: NOTIFY_INOUT_ANIM_DURATION_MS,
                                                                                    frameSize: BaseWidget.FrameSize,
                                                                                    startPos: new Point(0, -BaseWidget.FrameSize.Width),
                                                                                    endPos: new Point(0, 0));
        private static LCDWidget.Animators.LinearInOut footerAnimator = new LCDWidget.Animators.LinearInOut(inDurationMs: FOOTER_INOUT_ANIM_DURATION_MS,
                                                                                    holdDurationMs: FOOTER_DURATION_MS,
                                                                                    outDurationMs: FOOTER_INOUT_ANIM_DURATION_MS,
                                                                                    frameSize: BaseWidget.FrameSize,
                                                                                    startPos: new Point(0, BaseWidget.FrameSize.Height),
                                                                                    endPos: new Point(0, BaseWidget.FrameSize.Height - 20));
        private static LCDWidget.Animators.Linear oldWidgetAnimator = new LCDWidget.Animators.Linear(durationMs: WIDGET_TRANSITION_DURATION_MS, frameSize: BaseWidget.FrameSize);
        private static LCDWidget.Animators.Linear newWidgetAnimator = new LCDWidget.Animators.Linear(durationMs: WIDGET_TRANSITION_DURATION_MS, frameSize: BaseWidget.FrameSize);

        private static Font footerFont = new Font(FontFamily.GenericSansSerif, 8);
        private static StringFormat footerFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        private static List<LCDWidget.Animators.AbstractAnimator> animators = new List<LCDWidget.Animators.AbstractAnimator>() { oldWidgetAnimator, newWidgetAnimator, notifyAnimator, footerAnimator };

        static WidgetComposer()
        {
            WidgetManager.Notify += WidgetManager_Notify;
            WidgetManager.WidgetChanged += WidgetManager_WidgetChanged;
        }

        /// <summary>
        /// Converts the composed display image into a series of bytes.
        /// </summary>
        /// <returns>Converted display image.</returns>
        public static byte[] GetDisplayData()
        {
            return BitmapToRGBBytes(ComposeWidgets());
        }

        /// <summary>
        /// Handles the active widget being changed.
        /// </summary>
        /// <param name="oldWidget">Previous active widget.</param>
        /// <param name="newWidget">New active widget.</param>
        /// <param name="offset">Direction the active widget changed in.</param>
        private static void WidgetManager_WidgetChanged(BaseWidget oldWidget, BaseWidget newWidget, int offset)
        {
            oldWidgetAnimator.Image = oldWidget.RenderFrame();
            newWidgetAnimator.Image = newWidget.RenderFrame();
            if (offset >= 0)
            {
                oldWidgetAnimator.EndPos = new Point(-BaseWidget.FrameSize.Width, 0);
                newWidgetAnimator.StartPos = new Point(BaseWidget.FrameSize.Width, 0);
            }
            else
            {
                oldWidgetAnimator.EndPos = new Point(BaseWidget.FrameSize.Width, 0);
                newWidgetAnimator.StartPos = new Point(-BaseWidget.FrameSize.Width, 0);
            }
            oldWidgetAnimator.StartPos = newWidgetAnimator.EndPos = Point.Empty;

            oldWidgetAnimator.Start();
            newWidgetAnimator.Start();

            StartFooter(newWidget.Name, FOOTER_INOUT_ANIM_DURATION_MS, FOOTER_DURATION_MS);
        }

        /// <summary>
        /// Starts the display footer animation.
        /// </summary>
        /// <param name="message">Message to display on the footer.</param>
        /// <param name="inoutMs">Time taken to move the footer in and out of the visible area.</param>
        /// <param name="holdMs">Duration to hold the footer in place.</param>
        private static void StartFooter(string message, int inoutMs, int holdMs)
        {
            Bitmap footerBitmap = new Bitmap(BaseWidget.FrameSize.Width, 20);
            using Graphics g = Graphics.FromImage(footerBitmap);
            g.FillRectangle(new LinearGradientBrush(Point.Empty, new Point(0, footerBitmap.Height), Color.Transparent, Color.Black), new Rectangle(Point.Empty, footerBitmap.Size));
            g.DrawString(message, footerFont, Brushes.Black, new Rectangle(new Point(1, 1), footerBitmap.Size), footerFormat);
            g.DrawString(message, footerFont, Brushes.White, new Rectangle(Point.Empty, footerBitmap.Size), footerFormat);

            footerAnimator.Image = footerBitmap;
            footerAnimator.InDuration = footerAnimator.OutDuration = inoutMs;
            footerAnimator.HoldDuration = holdMs;
            footerAnimator.Start();
        }

        /// <summary>
        /// Handles incoming notifications and displays them on the screen.
        /// </summary>
        /// <param name="notification">The notification to draw.</param>
        private static void WidgetManager_Notify(Notification notification)
        {
            if (notification.Origin == WidgetManager.CurrentWidget) return;
            notifyAnimator.HoldDuration = notification.DisplayTime;
            notifyAnimator.Image = NotificationRenderer.RenderNotification(notification);
            notifyAnimator.Start();
            StartFooter(notification.Origin.Name, NOTIFY_INOUT_ANIM_DURATION_MS, notifyAnimator.HoldDuration);
        }

        /// <summary>
        /// Compose the active widget, and any animators currently in progress, into a single image.
        /// </summary>
        /// <returns>Composed frame.</returns>
        private static Bitmap ComposeWidgets()
        {
            Bitmap result = WidgetManager.CurrentWidget.RenderFrame();

            using Graphics g = Graphics.FromImage(result);

            foreach (LCDWidget.Animators.AbstractAnimator animator in animators)
            {
                if (animator.Complete == true) continue;
                g.DrawImageUnscaled(animator.RenderFrame(), Point.Empty);
            }

            return result;
        }

        /// <summary>
        /// Converts a Bitmap into a sequence of RGB bytes.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>Byte array containing image.</returns>
        private static byte[] BitmapToRGBBytes(Bitmap bitmap)
        {
            byte[] data = new byte[bitmap.Width * bitmap.Height * 3];
            int i = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    data[i++] = color.R;
                    data[i++] = color.G;
                    data[i++] = color.B;
                }
            }
            return data;
        }
    }
}
