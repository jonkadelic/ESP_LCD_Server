using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ESP_LCD_Server.Components
{
    public class Panel : BaseContainer
    {
        public ILayoutManager LayoutManager { get; set; }

        public Panel(ILayoutManager layoutManager = default, Brush background = default, Rectangle bounds = default, Spacing margin = default, Alignment horizontalAlignment = default, Alignment verticalAlignment = default, AutoSize horizontalAutoSize = default, AutoSize verticalAutoSize = default) : base(background, bounds, margin, horizontalAlignment, verticalAlignment, horizontalAutoSize, verticalAutoSize)
        {
            if (layoutManager == default)
            {
                LayoutManager = new BasicLayoutManager();
            }
            else
            {
                LayoutManager = layoutManager;
            }
        }

        public override Bitmap Paint()
        {
            Bitmap bitmap = new Bitmap(MarginBounds.Width, MarginBounds.Height);
            using Graphics g = Graphics.FromImage(bitmap);
            List<Rectangle> layouts = LayoutManager.GetLayout(MarginBounds.Size, children);
            for (int i = 0; i < children.Count; i++)
            {
                BaseComponent child = children[i];
                Bitmap childBitmap = child.Paint();
                g.DrawImageUnscaledAndClipped(childBitmap, layouts[i]);
            }

            return bitmap;
        }
    }
}
