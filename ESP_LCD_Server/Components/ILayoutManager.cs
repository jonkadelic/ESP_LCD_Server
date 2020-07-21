using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ESP_LCD_Server.Components
{
    public interface ILayoutManager
    {
        public List<Rectangle> GetLayout(Size layoutArea, List<BaseComponent> components);
    }
}
