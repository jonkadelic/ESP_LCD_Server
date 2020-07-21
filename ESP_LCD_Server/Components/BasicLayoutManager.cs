using System.Collections.Generic;
using System.Drawing;

namespace ESP_LCD_Server.Components
{
    public class BasicLayoutManager : ILayoutManager
    {
        public List<Rectangle> GetLayout(Size layoutArea, List<BaseComponent> components)
        {
            List<Rectangle> layouts = new List<Rectangle>();
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
                layout.Height = component.VerticalAutoSize switch
                {
                    AutoSize.None => component.Bounds.Size.Height,
                    AutoSize.Max => layoutArea.Height,
                    _ => component.Bounds.Size.Height
                };
                layout.X = component.HorizontalAlignment switch
                {
                    Alignment.None => component.Location.X,
                    Alignment.Min => 0,
                    Alignment.Center => (layoutArea.Width / 2) - (layout.Width / 2),
                    Alignment.Max => layoutArea.Width - layout.Width,
                    _ => component.Location.X
                };
                layout.Y = component.VerticalAlignment switch
                {
                    Alignment.None => component.Location.Y,
                    Alignment.Min => 0,
                    Alignment.Center => (layoutArea.Height / 2) - (layout.Height / 2),
                    Alignment.Max => layoutArea.Height - layout.Height,
                    _ => component.Location.Y
                };

                layouts.Add(layout);
            }

            return layouts;
        }
    }
}
