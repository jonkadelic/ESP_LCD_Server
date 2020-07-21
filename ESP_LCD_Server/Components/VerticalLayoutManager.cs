using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ESP_LCD_Server.Components
{
    public class VerticalLayoutManager : ILayoutManager
    {
        public List<Rectangle> GetLayout(Size layoutArea, List<BaseComponent> components)
        {
            List<Rectangle> layouts = new List<Rectangle>();
            int totalLayoutY = 0;
            for (int i = 0; i < components.Count; i++)
            {
                Rectangle layout = new Rectangle();
                BaseComponent component = components[i];

                layout.Width = component.HorizontalAutoSize switch
                {
                    AutoSize.None => component.Bounds.Size.Width,
                    AutoSize.Max => layoutArea.Width,
                    _ => component.Bounds.Size.Width
                };
                layout.Height = component.Size.Height;
                layout.X = component.HorizontalAlignment switch
                {
                    Alignment.None => component.Location.X,
                    Alignment.Min => 0,
                    Alignment.Center => (layoutArea.Width / 2) - (layout.Width / 2),
                    Alignment.Max => layoutArea.Width - layout.Width,
                    _ => component.Location.X
                };
                layout.Y = totalLayoutY;

                totalLayoutY += layout.Height;

                layouts.Add(layout);
            }

            return layouts;
        }
    }
}
