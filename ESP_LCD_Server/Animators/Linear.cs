using System.Drawing;

namespace ESP_LCD_Server.Animators
{
    public class Linear : AbstractAnimator
    {
        /// <summary>
        /// Image to translate from StartPos to EndPos through the animation's lifespan.
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
        /// Creates a new Linear Animator.
        /// </summary>
        /// <param name="durationMs">Length of the animation, in milliseconds.</param>
        /// <param name="frameSize">Dimensions of the area this animation should occupy.</param>
        /// <param name="image">Image to translate from StartPos to EndPos through the animation's lifespan.</param>
        /// <param name="startPos">Point Image should start the animation at.</param>
        /// <param name="endPos">Point Image should end the animation at.</param>
        public Linear(int durationMs = default, Size frameSize = default, Bitmap image = default, Point startPos = default, Point endPos = default) : base(durationMs, frameSize)
        {
            StartPos = startPos;
            EndPos = endPos;
            Image = image;
        }

        public override Bitmap RenderFrame()
        {
            Bitmap bitmap = new Bitmap(FrameSize.Width, FrameSize.Height);
            Size offset = new Size((int)((EndPos.X - StartPos.X) * Progress), (int)((EndPos.Y - StartPos.Y) * Progress));
            Point drawingPoint = StartPos + offset;

            using Graphics g = Graphics.FromImage(bitmap);
            g.DrawImageUnscaled(Image, drawingPoint);
            return bitmap;
        }
    }
}
