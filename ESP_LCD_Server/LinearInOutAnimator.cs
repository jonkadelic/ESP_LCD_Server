using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ESP_LCD_Server
{
    public class LinearInOutAnimator : Animator
    {
        private int _InDuration = 0;
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

        public Bitmap MovingImage { get; set; }
        public Point StartPos { get; set; }
        public Point EndPos { get; set; }

        public LinearInOutAnimator(int inDurationMs = default, int holdDurationMs = default, int outDurationMs = default, Size frameSize = default, Bitmap movingImage = default, Point startPos = default, Point endPos = default) : base(inDurationMs + holdDurationMs + outDurationMs, frameSize)
        {
            InDuration = inDurationMs;
            HoldDuration = holdDurationMs;
            OutDuration = outDurationMs;
            MovingImage = movingImage;
            StartPos = startPos;
            EndPos = endPos;
        }

        public override Bitmap RenderFrame()
        {
            Bitmap bitmap = GetTransparentBitmap();
            using Graphics g = Graphics.FromImage(bitmap);

            // If image is holding
            if (timerProgress > InDuration && timerProgress < (InDuration + HoldDuration))
            {
                g.DrawImageUnscaled(MovingImage, EndPos);
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
                g.DrawImageUnscaled(MovingImage, drawingPoint);
            }

            return bitmap;
        }
    }
}
