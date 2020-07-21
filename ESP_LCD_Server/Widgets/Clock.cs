using System;
using System.Drawing;
using System.Threading.Tasks;

namespace ESP_LCD_Server.Widgets
{
    public class Clock : BaseWidget
    {
        private const int DIGIT_CHANGE_TIME_MS = 100;
        private const int DIGIT_COUNT = 6;
        private const int DIGIT_ANIM_IN = 0;
        private const int DIGIT_ANIM_OUT = 1;

        private readonly Font digitFont = new Font(FontFamily.GenericSansSerif, 40);
        private readonly StringFormat digitFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        private readonly Size digitSize = new Size(30, 50);
        private readonly Point topLeft;

        private Rectangle[] digitRects = new Rectangle[DIGIT_COUNT];
        private Animators.Linear[,] digitAnimators = new Animators.Linear[DIGIT_COUNT, 2];

        private string lastTimeString;

        bool drawSeconds = false;

        public override string Name => "Clock";

        /// <summary>
        /// Creates a new Clock widget.
        /// </summary>
        public Clock()
        {
            topLeft = new Point(FrameSize.Width / 2 - digitSize.Width, FrameSize.Height / 2 - digitSize.Height);
            digitRects[0] = new Rectangle(topLeft, digitSize);
            for (int i = 1; i < DIGIT_COUNT; i++)
            {
                Rectangle lastRect = digitRects[i - 1];
                if (i % 2 == 0)
                {
                    digitRects[i] = new Rectangle(new Point(lastRect.X - digitSize.Width, lastRect.Y + digitSize.Height), digitSize);
                }
                else
                {
                    digitRects[i] = new Rectangle(new Point(lastRect.X + digitSize.Width, lastRect.Y), digitSize);
                }
            }

            for (int i = 0; i < DIGIT_COUNT; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    digitAnimators[i, j] = new Animators.Linear(durationMs: DIGIT_CHANGE_TIME_MS, frameSize: FrameSize);
                }
            }

            DateTime currentTime = DateTime.Now;
            lastTimeString = $"{currentTime.Hour:00}{currentTime.Minute:00}{currentTime.Second:00}";
        }

        public override Task HandleActionAsync()
        {
            drawSeconds = !drawSeconds;
            return Task.CompletedTask;
        }

        public override Bitmap RenderFrame()
        {
            Size offset = new Size(1, 0);
            int digitCount = DIGIT_COUNT - 2;
            if (drawSeconds)
            {
                offset.Height = -(digitSize.Height / 2);
                digitCount = DIGIT_COUNT;
            }

            DateTime currentTime = DateTime.Now;
            string currentTimeString = $"{currentTime.Hour:00}{currentTime.Minute:00}{currentTime.Second:00}";

            Bitmap bitmap = new Bitmap(FrameSize.Width, FrameSize.Height);
            using Graphics g = Graphics.FromImage(bitmap);
            g.FillRectangle(Brushes.Black, new Rectangle(Point.Empty, bitmap.Size));

            for (int i = 0; i < digitCount; i++)
            {
                bool drawDigit = true;

                if (lastTimeString[i] != currentTimeString[i] && digitAnimators[i, DIGIT_ANIM_IN].Complete == true)
                {
                    digitAnimators[i, DIGIT_ANIM_IN].Image = RenderDigit(currentTimeString[i]);
                    digitAnimators[i, DIGIT_ANIM_IN].StartPos = digitRects[i].Location - new Size(0, digitSize.Height) + offset;
                    digitAnimators[i, DIGIT_ANIM_IN].EndPos = digitRects[i].Location + offset;
                    digitAnimators[i, DIGIT_ANIM_IN].Start();
                    digitAnimators[i, DIGIT_ANIM_OUT].Image = RenderDigit(lastTimeString[i]);
                    digitAnimators[i, DIGIT_ANIM_OUT].StartPos = digitRects[i].Location + offset;
                    digitAnimators[i, DIGIT_ANIM_OUT].EndPos = digitRects[i].Location + new Size(0, digitSize.Height) + offset;
                    digitAnimators[i, DIGIT_ANIM_OUT].Start();
                }

                for (int j = 0; j < 2; j++)
                {
                    if (digitAnimators[i, j].Complete == false)
                    {
                        drawDigit = false;
                        Bitmap digit = digitAnimators[i, j].RenderFrame();
                        g.SetClip(new Rectangle(digitRects[i].Location + offset, digitRects[i].Size));
                        g.DrawImageUnscaled(digit, Point.Empty);
                    }
                }

                if (drawDigit)
                {
                    g.DrawImageUnscaled(RenderDigit(currentTimeString[i]), digitRects[i].Location + offset);
                }
            }

            lastTimeString = currentTimeString;

            return bitmap;
        }

        /// <summary>
        /// Renders a bitmap containing a clock digit.
        /// </summary>
        /// <param name="digit">Digit to render.</param>
        /// <returns>The rendered digit bitmap.</returns>
        private Bitmap RenderDigit(char digit)
        {
            Bitmap bitmap = new Bitmap(digitSize.Width, digitSize.Height);
            using Graphics g = Graphics.FromImage(bitmap);
            g.DrawString($"{digit}", digitFont, Brushes.White, new Rectangle(Point.Empty, digitSize), digitFormat);
            return bitmap;
        }
    }
}
