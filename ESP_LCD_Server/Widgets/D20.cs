using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;

namespace ESP_LCD_Server.Widgets
{
    public class D20 : BaseWidget
    {
        private string lastRolled = "?";
        private List<GraphicsPath> d20Paths;
        private Font numFont = new Font(FontFamily.GenericSansSerif, 15);
        private StringFormat numFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        private int flashCount = 0;
        bool nat20 = false;
        bool flash = false;
        public override string Name => "D20";

        /// <summary>
        /// Creates a new D20 roller digit.
        /// </summary>
        public D20()
        {
            float size = FrameSize.Width * 0.75f;
            d20Paths = GetD20Paths(new SizeF(size, size));
            Matrix translate = new Matrix();
            translate.Translate((FrameSize.Width - size) / 2, (FrameSize.Height - size) / 2);
            foreach (GraphicsPath path in d20Paths)
            {
                path.Transform(translate);
            }
        }

        public override Task HandleActionAsync()
        {
            Task.Run(() =>
            {
                Random random = new Random();
                int length = random.Next(3, 10);
                for (int i = 0; i < length; i++)
                {
                    lastRolled = (random.Next(20) + 1).ToString();
                    Thread.Sleep(random.Next(100, 300));
                }
                if (lastRolled == "20") nat20 = true; 
            }).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        public override Bitmap RenderFrame()
        {
            Bitmap bitmap = new Bitmap(FrameSize.Width, FrameSize.Height);
            using Graphics g = Graphics.FromImage(bitmap);
            Brush bg = flash ? Brushes.White : Brushes.Black;
            g.FillRectangle(bg, new Rectangle(Point.Empty, FrameSize));
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.FillPath(Brushes.Red, d20Paths[0]);
            d20Paths.ForEach((x) => g.DrawPath(new Pen(Color.DarkRed, 2), x));

            g.DrawString(lastRolled, numFont, Brushes.White, new Rectangle(Point.Empty, bitmap.Size), numFormat);

            if (nat20)
            {
                if (flashCount > 10)
                {
                    nat20 = false;
                    flash = false;
                    flashCount = 0;
                }
                else
                {
                    flash = !flash;
                    flashCount++;
                }
            }

            return bitmap;
        }

        /// <summary>
        /// Generates a GraphicsPath in the shape of a D20.
        /// </summary>
        /// <param name="size">Size to generate the D20 at.</param>
        /// <returns>A number of GraphicsPaths comprising the D20 path.</returns>
        private List<GraphicsPath> GetD20Paths(SizeF size)
        {
            List<GraphicsPath> paths = new List<GraphicsPath>();
            PointF[] points = new PointF[] { new PointF(size.Width * 0.5f, 0),
                                             new PointF(size.Width, size.Height * 0.25f),
                                             new PointF(size.Width, size.Height * 0.75f),
                                             new PointF(size.Width * 0.5f, size.Height),
                                             new PointF(0, size.Height * 0.75f),
                                             new PointF(0, size.Height * 0.25f),
                                             new PointF(size.Width * 0.5f, size.Height * 0.22f),
                                             new PointF(size.Width * 0.75f, size.Height * 0.66f),
                                             new PointF(size.Width * 0.25f, size.Height * 0.66f)
                                           };
            paths.Add(new GraphicsPath());
            paths.Add(new GraphicsPath());
            paths.Add(new GraphicsPath());
            paths.Add(new GraphicsPath());
            paths.Add(new GraphicsPath());
            paths[0].AddPolygon(new PointF[] { points[0], points[1], points[2], points[3], points[4], points[5], points[0] });
            paths[1].AddPolygon(new PointF[] { points[5], points[6], points[1], points[7], points[3], points[8], points[6], points[7], points[8], points[5] });
            paths[2].AddLine(points[0], points[6]);
            paths[3].AddLine(points[2], points[7]);
            paths[4].AddLine(points[4], points[8]);

            return paths;
        }
    }
}
