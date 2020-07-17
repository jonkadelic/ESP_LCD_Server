using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ESP_LCD_Server
{
    public class LinearAnimator : Animator
    {
        public Bitmap MovingImage { get; set; }
        public Point StartPos { get; set; }
        public Point EndPos { get; set; }

        public LinearAnimator(int durationMs = default, Size frameSize = default, Bitmap movingImage = default, Point startPos = default, Point endPos = default) : base(durationMs, frameSize)
        {
            StartPos = startPos;
            EndPos = endPos;
            MovingImage = movingImage;
        }

        public override Bitmap RenderFrame()
        {
            Bitmap bitmap = GetTransparentBitmap();
            Point offset = new Point((int)((EndPos.X - StartPos.X) * Progress), (int)((EndPos.Y - EndPos.Y) * Progress));
            Point drawingPoint = new Point(StartPos.X, StartPos.Y);
            drawingPoint.Offset(offset);
            using Graphics g = Graphics.FromImage(bitmap);
            g.DrawImageUnscaled(MovingImage, drawingPoint);
            return bitmap;
        }
    }
}
