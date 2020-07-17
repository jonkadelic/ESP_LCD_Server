using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ESP_LCD_Server
{
    public abstract class AbstractNotifyingPage : AbstractPage
    {
        public event NotifyEventHandler Notify;
        public delegate void NotifyEventHandler(AbstractNotifyingPage sender);

        public abstract int NotifyDurationMs { get; }

        protected void OnNotify()
        {
            NotifyEventHandler handler = Notify;
            handler?.Invoke(this);
        }

        public abstract Bitmap RenderNotifyFrame();
    }

}
