using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ESP_LCD_Server
{
    public static class PageComposer
    {
        private const int ANIMATION_INTERVAL_MS = 10;
        private const int PAGE_TRANSITION_DURATION_MS = 250;
        private const int NOTIFY_INOUT_ANIM_DURATION_MS = 250;
        private const int FOOTER_DURATION_MS = 2000;
        private const int FOOTER_INOUT_ANIM_DURATION_MS = 100;

        private static LinearInOutAnimator notifyAnimator = new LinearInOutAnimator(inDurationMs: NOTIFY_INOUT_ANIM_DURATION_MS,
                                                                                    outDurationMs: NOTIFY_INOUT_ANIM_DURATION_MS,
                                                                                    frameSize: AbstractPage.FrameSize,
                                                                                    startPos: new Point(0, -AbstractPage.FRAME_WIDTH),
                                                                                    endPos: new Point(0, 0));
        private static LinearInOutAnimator footerAnimator = new LinearInOutAnimator(inDurationMs: FOOTER_INOUT_ANIM_DURATION_MS,
                                                                                    outDurationMs: FOOTER_INOUT_ANIM_DURATION_MS,
                                                                                    frameSize: AbstractPage.FrameSize,
                                                                                    startPos: new Point(0, AbstractPage.FRAME_HEIGHT),
                                                                                    endPos: new Point(0, AbstractPage.FRAME_HEIGHT - 20));
        private static AbstractPage notifyingPage = null;
        private static AbstractPage oldPage = null;
        private static AbstractPage newPage = null;
        private static bool pageMovingRight;
        private static System.Timers.Timer transitionTimer = null;
        private static int transitionTimerProgress = 0;
        private static System.Threading.Timer footerTimer = null;

        private static Font footerFont = new Font(FontFamily.GenericSansSerif, 8);
        private static StringFormat footerFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        static PageComposer()
        {
            PageManager.Notify += PageManager_Notify;
            PageManager.PageChanged += PageManager_PageChanged;
        }
        
        public static byte[] GetDisplayData()
        {
            return BitmapToRGBBytes(ComposePage());
        }

        private static void PageManager_PageChanged(AbstractPage oldPage, AbstractPage newPage, int pageChangeOffset)
        {
            PageComposer.oldPage = oldPage;
            PageComposer.newPage = newPage;
            pageMovingRight = (pageChangeOffset >= 0);
            transitionTimerProgress = 0;
            if (transitionTimer != null) transitionTimer.Dispose();
            transitionTimer = new System.Timers.Timer(ANIMATION_INTERVAL_MS)
            {
                AutoReset = true
            };
            transitionTimer.Elapsed += (sender, args) => {
                transitionTimerProgress += ANIMATION_INTERVAL_MS;
                if (transitionTimerProgress >= PAGE_TRANSITION_DURATION_MS)
                {
                    PageComposer.oldPage = null;
                    PageComposer.newPage = null;
                    transitionTimer.Dispose();
                    transitionTimer = null;
                }
            };
            transitionTimer.Start();
            if (footerTimer != null) footerTimer.Dispose();
            footerTimer = new System.Threading.Timer((args) => footerTimer = null, null, FOOTER_DURATION_MS, Timeout.Infinite);
        }

        private static void PageManager_Notify(AbstractNotifyingPage page)
        {
            if (page == PageManager.CurrentPage) return;
            notifyingPage = page;
            notifyAnimator.HoldDuration = page.NotifyDurationMs;
            notifyAnimator.MovingImage = page.RenderNotifyFrame();
            notifyAnimator.Start();
        }

        private static Bitmap ComposePage()
        {
            Bitmap result;
            if (oldPage != null && newPage != null && transitionTimer != null)
            {
                Bitmap oldPageBitmap = oldPage.RenderFrame();
                Bitmap newPageBitmap = newPage.RenderFrame();
                int width = oldPageBitmap.Width;
                int height = oldPageBitmap.Height;
                result = new Bitmap(width, height);
                float animProgress = (float)transitionTimerProgress / PAGE_TRANSITION_DURATION_MS;
                using Graphics gfx = Graphics.FromImage(result);
                if (pageMovingRight)
                {
                    int splitX = (int)((1.0f - animProgress) * width);
                    gfx.DrawImageUnscaled(oldPageBitmap, splitX - width, 0);
                    gfx.DrawImageUnscaled(newPageBitmap, splitX, 0);
                }
                else
                {
                    int splitX = (int)(animProgress * width);
                    gfx.DrawImageUnscaled(newPageBitmap, splitX - width, 0);
                    gfx.DrawImageUnscaled(oldPageBitmap, splitX, 0);
                }
            }
            else
            {
                result = PageManager.CurrentPage.RenderFrame();
            }

            //if (footerTimer != null)
            //{
            //    int footerTop = result.Height - 20;
            //    Graphics g = Graphics.FromImage(result);
            //    g.FillRectangle(new SolidBrush(Color.FromArgb(127, 0, 0, 0)), 0, footerTop, result.Width, 20);
            //    g.DrawString(PageManager.CurrentPage.Name, footerFont, Brushes.Black, new Rectangle(1, footerTop + 1, result.Width, 20), footerFormat);
            //    g.DrawString(PageManager.CurrentPage.Name, footerFont, Brushes.White, new Rectangle(0, footerTop, result.Width, 20), footerFormat);
            //}

            using Graphics g = Graphics.FromImage(result);

            foreach (Animator animator in Animator.AnimatorsInProgress)
            {
                Bitmap animFrame = animator.RenderFrame();
                if (animator == notifyAnimator)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(200, Color.Black)), new Rectangle(Point.Empty, result.Size));
                    g.DrawImageUnscaled(animFrame, Point.Empty);
                    g.DrawString(notifyingPage.Name, footerFont, Brushes.Black, new Rectangle(1, result.Height - 40 + 1, result.Width, 40), footerFormat);
                    g.DrawString(notifyingPage.Name, footerFont, Brushes.White, new Rectangle(0, result.Height - 40, result.Width, 40), footerFormat);
                }
                else
                {
                    g.DrawImageUnscaled(animFrame, Point.Empty);
                }
            }

            return result;
        }

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
