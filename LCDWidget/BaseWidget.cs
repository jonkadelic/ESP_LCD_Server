using System.Drawing;
using System.Threading.Tasks;

namespace LCDWidget
{
    public abstract class BaseWidget
    {
        protected const int BYTES_PER_PIXEL = 3;
        /// <summary>
        /// Dimensions of a widget frame in pixels.
        /// </summary>
        public static Size FrameSize => new Size(128, 160);

        public abstract int Priority { get; }

        /// <summary>
        /// Widget's name, used for display purposes.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Renders a single frame of the widget.
        /// </summary>
        /// <returns>The generated frame.</returns>
        public abstract Bitmap RenderFrame();

        /// <summary>
        /// Handles an incoming action on the widget asynchronously.
        /// </summary>
        /// <returns>Task status.</returns>
        public abstract Task HandleActionAsync();
    }
}
