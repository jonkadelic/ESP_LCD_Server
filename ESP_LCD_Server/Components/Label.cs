using MQTTnet.Client.Publishing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ESP_LCD_Server.Components
{
    public class Label : BaseComponent
    {
        private string _Text;
        public string Text
        {
            get => _Text;
            set
            {
                _Text = value;
                if (AutoWidth) UpdateWidth();
            }
        }
        private Font _Font;
        public Font Font
        {
            get => _Font;
            set
            {
                _Font = new Font(value.FontFamily, FontHeight);
                UpdateHeight();
                if (AutoWidth) UpdateWidth();
            }
        }
        private int _FontHeight;
        public int FontHeight
        {
            get => _FontHeight;
            set
            {
                _FontHeight = value;
                Font = new Font(Font.FontFamily, FontHeight);
            }
        }
        private bool _AutoWidth = false;
        public bool AutoWidth
        {
            get => _AutoWidth;
            set
            {
                _AutoWidth = value;
                UpdateWidth();
            }
        }
        public Brush Foreground { get; set; }
        public bool DropShadow { get; set; }
        private StringFormat format = new StringFormat(StringFormatFlags.NoWrap) { Trimming = StringTrimming.EllipsisCharacter };

        public Label(string text, Font font = default, int fontHeight = 12, bool autoWidth = false, Brush foreground = default, bool dropShadow = false, Brush background = default, Rectangle bounds = default, Spacing margin = default, Alignment horizontalAlignment = default, Alignment verticalAlignment = default, AutoSize horizontalAutoSize = default, AutoSize verticalAutoSize = default) : base(background, bounds, margin, horizontalAlignment, verticalAlignment, horizontalAutoSize, verticalAutoSize)
        {
            AutoWidth = autoWidth;
            _FontHeight = fontHeight;
            if (font == default) Font = new Font(FontFamily.GenericSansSerif, fontHeight);
            Text = text;
            Foreground = foreground;
            DropShadow = dropShadow;
        }
        public override Bitmap Paint()
        {
            Bitmap bitmap = new Bitmap(Size.Width, Size.Height);
            using Graphics g = Graphics.FromImage(bitmap);
            g.DrawString(Text, Font, Foreground, MarginBounds, format);

            return bitmap;
        }
        private void UpdateWidth()
        {
            using Graphics g = Graphics.FromImage(new Bitmap(1, 1)); // null graphics because for some reason MeasureString isn't static?????????????
            Size = new Size((int)g.MeasureString(_Text, Font).Width + Margin.Size.Width, Size.Height);
        }

        private void UpdateHeight()
        {
            using Graphics g = Graphics.FromImage(new Bitmap(1, 1)); // null graphics because for some reason MeasureString isn't static?????????????
            Size = new Size(Size.Width, (int)g.MeasureString(Text, Font).Height + Margin.Size.Height);
        }
    }
}
