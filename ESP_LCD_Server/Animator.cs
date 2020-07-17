using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Timers;

namespace ESP_LCD_Server
{
    public abstract class Animator
    {
        private const int TIMER_FIRE_INTERVAL = 10;
        protected int timerProgress = 0;
        private Timer timer;

        public static List<Animator> AnimatorsInProgress { get; } = new List<Animator>();

        public bool Complete { get; protected set; }
        public Size FrameSize { get; set; }
        public virtual int Duration { get; set; }
        protected float Progress => (float)timerProgress / Duration;

        public Animator(int durationMs = default, Size frameSize = default)
        {
            Duration = durationMs;
            FrameSize = frameSize;
        }

        public void Start()
        {
            if (timer == null)
            {
                Complete = false;
                timerProgress = 0;
                timer = new Timer(TIMER_FIRE_INTERVAL);
                timer.Elapsed += Timer_Elapsed;
                timer.AutoReset = true;
                timer.Start();
                AnimatorsInProgress.Add(this);
            }
            else
            {
                timerProgress = Duration + 1;
                Timer_Elapsed(null, null);
                Start();
            }
        }

        public abstract Bitmap RenderFrame();

        protected Bitmap GetTransparentBitmap()
        {
            Bitmap bitmap = new Bitmap(FrameSize.Width, FrameSize.Height);
            using Graphics g = Graphics.FromImage(bitmap);
            using SolidBrush transparentBrush = new SolidBrush(Color.FromArgb(0, 255, 255, 255));
            g.FillRectangle(transparentBrush, new Rectangle(new Point(0, 0), bitmap.Size));
            return bitmap;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (timerProgress >= Duration)
            {
                timer.Dispose();
                AnimatorsInProgress.Remove(this);
                Complete = true;
                timer = null;
                return;
            }
            timerProgress += TIMER_FIRE_INTERVAL;
        }
    }
}
