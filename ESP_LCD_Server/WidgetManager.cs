using LCDWidget;
using System;
using System.Collections.Generic;

namespace ESP_LCD_Server
{
    public static class WidgetManager
    {
        private static readonly List<BaseWidget> widgets = new List<BaseWidget>();

        /// <summary>
        /// Widget that's currently being displayed.
        /// </summary>
        public static BaseWidget CurrentWidget { get; private set; }
        /// <summary>
        /// Number of widgets in the WidgetManager.
        /// </summary>
        public static int WidgetCount => widgets.Count;
        /// <summary>
        /// Widget has raised a notification.
        /// </summary>
        public static event NotifyEventHandler Notify;
        /// <summary>
        /// Called when a notification is raised by a widget.
        /// </summary>
        /// <param name="widget">The widget that raised the notification.</param>
        public delegate void NotifyEventHandler(Notification notification);
        /// <summary>
        /// Active widget has changed.
        /// </summary>
        public static event WidgetChangedEventHandler WidgetChanged;
        /// <summary>
        /// Called when the active widget changes.
        /// </summary>
        /// <param name="oldWidget">Previous active widget.</param>
        /// <param name="newWidget">Mew active widget.</param>
        /// <param name="offset">Direction the active widget has changed in.</param>
        public delegate void WidgetChangedEventHandler(BaseWidget oldWidget, BaseWidget newWidget, int offset);

        public static void LoadWidgets()
        {
            WidgetLoader.LoadWidgets();
            foreach (Type type in WidgetLoader.WidgetTypes)
            {
                BaseWidget widget = (BaseWidget)Activator.CreateInstance(type);
                AddWidget(widget);
            }

            widgets.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        }

        /// <summary>
        /// Registers a widget with the Widget Manager.
        /// </summary>
        /// <param name="widget">Widget to register.</param>
        public static void AddWidget(BaseWidget widget)
        {
            widgets.Add(widget);
            if (widget is BaseNotifyingWidget)
            {
                (widget as BaseNotifyingWidget).Notify += OnNotify;
            }
            if (CurrentWidget == null) CurrentWidget = widget;
        }

        /// <summary>
        /// Handles incoming widget notifications.
        /// </summary>
        /// <param name="sender">Widget that sent the notification.</param>
        private static void OnNotify(LCDWidget.Notification notification)
        {
            NotifyEventHandler handler = Notify;
            handler?.Invoke(notification);
        }

        /// <summary>
        /// Switches the active widget to the next in the sequence.
        /// </summary>
        public static void NextWidget() => OffsetCurrentWidget(1);
        /// <summary>
        /// Switches the active widget to the last in the sequence.
        /// </summary>
        public static void LastWidget() => OffsetCurrentWidget(-1);
        /// <summary>
        /// Triggers the action of the active event.
        /// </summary>
        public static void Action() => CurrentWidget.HandleActionAsync().ConfigureAwait(false);

        /// <summary>
        /// Offsets the active widget by a given number.
        /// </summary>
        /// <param name="offset">Number of indices to offset the current widget by.</param>
        private static void OffsetCurrentWidget(int offset)
        {
            if (CurrentWidget == null) return;
            if (offset == 0) return;

            int newWidgetIndex = (widgets.IndexOf(CurrentWidget) + offset) % widgets.Count;
            if (newWidgetIndex < 0) newWidgetIndex += widgets.Count;

            BaseWidget oldWidget = CurrentWidget;
            BaseWidget newWidget = widgets[newWidgetIndex];


            WidgetChangedEventHandler handler = WidgetChanged;
            handler?.Invoke(oldWidget, newWidget, offset);

            CurrentWidget = newWidget;

        }


    }
}
