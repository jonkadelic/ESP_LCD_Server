using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;

namespace ESP_LCD_Server.Components
{
    public abstract class BaseContainer : BaseComponent, ICollection<BaseComponent>
    {
        protected readonly List<BaseComponent> children;

        public BaseContainer(Brush background = default, Rectangle bounds = default, Spacing margin = default, Alignment horizontalAlignment = default, Alignment verticalAlignment = default, AutoSize horizontalAutoSize = default, AutoSize verticalAutoSize = default) : base(background, bounds, margin, horizontalAlignment, verticalAlignment, horizontalAutoSize, verticalAutoSize)
        {
            children = new List<BaseComponent>();
        }

        public int Count => children.Count;

        public bool IsReadOnly => false;

        public void Add(BaseComponent item)
        {
            item.Parent = this;
            children.Add(item);
        }

        public void Clear()
        {
            ((ICollection<BaseComponent>)children).Clear();
        }

        public bool Contains(BaseComponent item)
        {
            return ((ICollection<BaseComponent>)children).Contains(item);
        }

        public void CopyTo(BaseComponent[] array, int arrayIndex)
        {
            ((ICollection<BaseComponent>)children).CopyTo(array, arrayIndex);
        }

        public IEnumerator<BaseComponent> GetEnumerator()
        {
            return ((IEnumerable<BaseComponent>)children).GetEnumerator();
        }

        public bool Remove(BaseComponent item)
        {
            return ((ICollection<BaseComponent>)children).Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)children).GetEnumerator();
        }
    }
}
