using System.Drawing;
using System.Timers;

namespace ESP_LCD_Server.Animators
{
    public abstract class AbstractAnimator
    {
        private static Timer timer;
        private const int TIMER_FIRE_INTERVAL = 10;
        
        /// <summary>
        /// Time since animation began, in milliseconds.
        /// </summary>
        protected int timerProgress = 0;

        /// <summary>
        /// Whether the animator has finished animating.
        /// </summary>
        public bool Complete { get; protected set; } = true;
        /// <summary>
        /// Size of the animator frame to generate.
        /// </summary>
        public Size FrameSize { get; set; }
        /// <summary>
        /// Duration the animation should last in milliseconds.
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// Progress through the animation's runtime, as a percentage (0.0 - 1.0).
        /// </summary>
        public float Progress => (float)timerProgress / Duration;

        static AbstractAnimator()
        {
            timer = new Timer(TIMER_FIRE_INTERVAL);
            timer.AutoReset = true;
            timer.Start();
        }

        /// <summary>
        /// Creates a new Animator with the specified duration and frame size.
        /// </summary>
        /// <param name="durationMs">Duration of the animation in milliseconds.</param>
        /// <param name="frameSize">Dimensions of the frame to render in pixels.</param>
        public AbstractAnimator(int durationMs = default, Size frameSize = default)
        {
            Duration = durationMs;
            FrameSize = frameSize;
            timer.Elapsed += Timer_Elapsed;
        }

        /// <summary>
        /// Starts the animator's animation sequence.
        /// </summary>
        public void Start()
        {
            if (timerProgress != 0)
            {
                timerProgress = Duration + 1;
                Timer_Elapsed(null, null);
            }
            Complete = false;
            timerProgress = 0;
        }

        /// <summary>
        /// Renders a single frame of the animation, based on its current progress.
        /// </summary>
        /// <returns></returns>
        public abstract Bitmap RenderFrame();

        /// <summary>
        /// Handle a tick from the timer, to update the animation's progress and check if it has finished.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (timerProgress >= Duration)
            {
                Complete = true;
                return;
            }
            timerProgress += TIMER_FIRE_INTERVAL;
        }
    }
}
