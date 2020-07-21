namespace ESP_LCD_Server.Widgets
{
    public abstract class BaseNotifyingWidget : BaseWidget
    {
        /// <summary>
        /// Event that triggers a notification to be displayed.
        /// </summary>
        public event NotifyEventHandler Notify;
        public delegate void NotifyEventHandler(Notification notification);

        /// <summary>
        /// Duration the notification is held on screen.
        /// </summary>
        public abstract int NotifyDurationMs { get; }

        /// <summary>
        /// Dispatches a notification.
        /// </summary>
        protected void OnNotify(Notification notification)
        {
            NotifyEventHandler handler = Notify;
            handler?.Invoke(notification);
        }
    }

}
