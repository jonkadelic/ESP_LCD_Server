using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace ESP_LCD_Server
{
    public abstract class AbstractPage
    {
        protected const int BYTES_PER_PIXEL = 3;
        public const int FRAME_WIDTH = 128;
        public const int FRAME_HEIGHT = 160;
        public static Size FrameSize { get; } = new Size(FRAME_WIDTH, FRAME_HEIGHT);

        public event NotifyEventHandler Notify;
        public delegate void NotifyEventHandler(AbstractPage sender);

        public abstract string Name { get; }
        public abstract int NotifyDurationMs { get; }

        protected void OnNotify()
        {
            NotifyEventHandler handler = Notify;
            handler?.Invoke(this);
        }

        public abstract Bitmap RenderFrame();

        public abstract Task HandleActionAsync();
    }
}
