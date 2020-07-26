using System.Drawing;

namespace LCDWidget.Animators
{
    public class LinearInOut : AbstractAnimator
    {
        private int _InDuration = 0;
        /// <summary>
        /// The time in milliseconds it should take for Image to move from StartPos to EndPos.
        /// </summary>
        public int InDuration 
        {
            get
            {
                return _InDuration;
            }
            set
            {
                _InDuration = value;
                Duration = InDuration + HoldDuration + OutDuration;
            }
        }
        private int _HoldDuration = 0;
        /// <summary>
        /// The time in milliseconds Image should remain at EndPos.
        /// </summary>
        public int HoldDuration
        {
            get
            {
                return _HoldDuration;
            }
            set
            {
                _HoldDuration = value;
                Duration = InDuration + HoldDuration + OutDuration;
            }
        }
        private int _OutDuration = 0;
        /// <summary>
        /// The time in milliseconds it should take for Image to move back from EndPos to StartPos.
        /// </summary>
        public int OutDuration
        {
            get
            {
                return _OutDuration;
            }
            set
            {
                _OutDuration = value;
                Duration = InDuration + HoldDuration + OutDuration;
            }
        }

        /// <summary>
        /// Image to translate from StartPos to EndPos, then hold at EndPos, then translate back to StartPos, through the animation's lifespan.
        /// </summary>
        public Bitmap Image { get; set; }
        /// <summary>
        /// Point Image should start the animation at.
        /// </summary>
        public Point StartPos { get; set; }
        /// <summary>
        /// Point Image should end the animation at.
        /// </summary>
        public Point EndPos { get; set; }

        /// <summary>
        /// Creates a new Linear In-Out Animator.
        /// </summary>
        /// <param name="inDurationMs">The time in milliseconds it should take for Image to move from StartPos to EndPos.</param>
        /// <param name="holdDurationMs">The time in milliseconds Image should remain at EndPos.</param>
        /// <param name="outDurationMs">The time in milliseconds it should take for Image to move back from EndPos to StartPos.</param>
        /// <param name="frameSize">Dimensions of the area this animation should occupy.</param>
        /// <param name="image">Image to translate from StartPos to EndPos, then hold at EndPos, then translate back to StartPos, through the animation's lifespan.</param>
        /// <param name="startPos">Point Image should start the animation at.</param>
        /// <param name="endPos">Point Image should end the animation at.</param>
        public LinearInOut(int inDurationMs = default, int holdDurationMs = default, int outDurationMs = default, Size frameSize = default, Bitmap image = default, Point startPos = default, Point endPos = default) : base(inDurationMs + holdDurationMs + outDurationMs, frameSize)
        {
            InDuration = inDurationMs;
            HoldDuration = holdDurationMs;
            OutDuration = outDurationMs;
            Image = image;
            StartPos = startPos;
            EndPos = endPos;
        }

        public override Bitmap RenderFrame()
        {
            Bitmap bitmap = new Bitmap(FrameSize.Width, FrameSize.Height);
            using Graphics g = Graphics.FromImage(bitmap);

            // If image is holding
            if (timerProgress > InDuration && timerProgress < (InDuration + HoldDuration))
            {
                g.DrawImageUnscaled(Image, EndPos);
            }
            else
            {
                float progress;
                // If image is moving in
                if (timerProgress <= InDuration)
                {
                    progress = timerProgress / (float)InDuration;
                }
                // If image is moving out
                else
                {
                    progress = 1 - ((timerProgress - (InDuration + HoldDuration)) / (float)OutDuration);
                }
                Point offset = new Point((int)((EndPos.X - StartPos.X) * progress), (int)((EndPos.Y - StartPos.Y) * progress));
                Point drawingPoint = new Point(StartPos.X, StartPos.Y);
                drawingPoint.Offset(offset);
                g.DrawImageUnscaled(Image, drawingPoint);
            }

            return bitmap;
        }
    }
}
