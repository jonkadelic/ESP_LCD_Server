using System.Collections.Generic;
using System.Drawing;

namespace ESP_LCD_Server.Components
{
    public class HorizontalLayoutManager : ILayoutManager
    {
        public List<Rectangle> GetLayout(Size layoutArea, List<BaseComponent> components)
        {
            List<Rectangle> layouts = new List<Rectangle>();
            int totalLayoutX = 0;
            for (int i = 0; i < components.Count; i++)
            {
                Rectangle layout = new Rectangle();
                BaseComponent component = components[i];

                layout.Width = component.Size.Width;
                layout.Height = component.VerticalAutoSize switch
                {
                    AutoSize.None => component.Bounds.Size.Height,
                    AutoSize.Max => layoutArea.Height,
                    _ => component.Bounds.Size.Height
                };
                layout.X = totalLayoutX;
                layout.Y = component.VerticalAlignment switch
                {
                    Alignment.None => component.Location.Y,
                    Alignment.Min => 0,
                    Alignment.Center => (layoutArea.Height / 2) - (layout.Height / 2),
                    Alignment.Max => layoutArea.Height - layout.Height,
                    _ => component.Location.Y
                };

                totalLayoutX += layout.Width;

                layouts.Add(layout);
            }

            return layouts;
        }
    }
}
