using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;

namespace ESP_LCD_Server.Components
{
    public enum Alignment
    {
        None,
        Min,
        Center,
        Max
    }

    public enum AutoSize
    {
        None,
        Max
    }

    public class Spacing
    {
        public int Left { get; set; } = 0;
        public int Right { get; set; } = 0;
        public int Top { get; set; } = 0;
        public int Bottom { get; set; } = 0;
        public Size Size => new Size(Left + Right, Top + Bottom);

        public Spacing(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
    }

    public abstract class BaseComponent
    {
        public Alignment HorizontalAlignment { get; set; }
        public Alignment VerticalAlignment { get; set; }
        public AutoSize HorizontalAutoSize { get; set; }
        public AutoSize VerticalAutoSize { get; set; }
        public Rectangle Bounds { get; set; }
        public Rectangle MarginBounds => new Rectangle(new Point(Margin.Left, Margin.Top), Size - Margin.Size);
        public Brush Background { get; set; }

        public Size Size
        {
            get => Bounds.Size;
            set => Bounds = new Rectangle(Bounds.Location, value);
        }
        public Point Location
        {
            get => Bounds.Location;
            set => Bounds = new Rectangle(value, Bounds.Size);
        }
        public Spacing Margin { get; set; }
        public BaseContainer Parent { get; set; }

        public BaseComponent(Brush background = default, Rectangle bounds = default, Spacing margin = default, Alignment horizontalAlignment = default, Alignment verticalAlignment = default, AutoSize horizontalAutoSize = default, AutoSize verticalAutoSize = default)
        {
            Background = background;
            Bounds = bounds;
            Margin = margin;
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
            HorizontalAutoSize = horizontalAutoSize;
            VerticalAutoSize = verticalAutoSize;
        }

        public abstract Bitmap Paint();
    }
}
