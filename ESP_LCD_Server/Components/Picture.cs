using SuperfastBlur;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace ESP_LCD_Server.Components
{
    public class Picture : BaseComponent
    {
        
        public Bitmap Image { get; set; }
        public bool DrawCircular { get; set; }
        public int BlurLevel { get; set; }

        public Picture(Bitmap image, bool drawCircular = false, int blurLevel = 0, Brush background = default, Rectangle bounds = default, Spacing margin = default, Alignment horizontalAlignment = default, Alignment verticalAlignment = default, AutoSize horizontalAutoSize = default, AutoSize verticalAutoSize = default) : base(background, bounds, margin, horizontalAlignment, verticalAlignment, horizontalAutoSize, verticalAutoSize)
        {
            Image = image;
            DrawCircular = drawCircular;
            BlurLevel = blurLevel;
        }

        public override Bitmap Paint()
        {
            Bitmap bitmap = new Bitmap(Size.Width, Size.Height);
            Bitmap image = Image;
            using Graphics g = Graphics.FromImage(bitmap);
            g.FillRectangle(Background, MarginBounds);

            if (DrawCircular)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(MarginBounds);
                g.SetClip(path);
            }

            if (BlurLevel != 0)
            {
                image = new GaussianBlur(Image).Process(BlurLevel);
            }

            g.DrawImage(image, MarginBounds);

            return bitmap;
        }

    }
}
