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
        protected const int FRAME_WIDTH = 128;
        protected const int FRAME_HEIGHT = 160;
        protected bool needsFullRefresh = false;
        protected bool frameInUse = false;

        public event NotifyEventHandler Notify;
        public delegate void NotifyEventHandler(AbstractPage sender);

        public abstract string Name { get; }
        public abstract int NotifyDurationMs { get; }
        private Bitmap _frame;
        public Bitmap Frame
        {
            get
            {
                while (frameInUse) ;
                //Console.WriteLine($"Took hold of frame in {GetType().Name}.");
                frameInUse = true;
                return _frame;
            }
            protected set
            {
                while (frameInUse) ;
                frameInUse = true;
                _frame = value;
                frameInUse = false;
            }
        }

        public void ReleaseFrame()
        {
            //Console.WriteLine($"Released frame in {GetType().Name}.");
            frameInUse = false;
        }

        protected void OnNotify()
        {
            NotifyEventHandler handler = Notify;
            handler?.Invoke(this);
        }

        public abstract Task RenderFrameAsync();

        public abstract Task HandleActionAsync();
    }
}
