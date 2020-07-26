using System.Drawing;

namespace LCDWidget
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
            Logger.Log($"Raising notification.", GetType());
            NotifyEventHandler handler = Notify;
            handler?.Invoke(notification);
        }
    }

    public class Notification
    {
        private const int DEFAULT_DISPLAY_TIME_MS = 3000;

        /// <summary>
        /// Represents the method used to draw the icon.
        /// </summary>
        public enum ICON_STYLE
        {
            /// <summary>
            /// Icon is drawn as a square.
            /// </summary>
            SQUARE,
            /// <summary>
            /// Icon is drawn as a circle.
            /// </summary>
            ROUND
        }

        /// <summary>
        /// Widget that sent this notification.
        /// </summary>
        public BaseNotifyingWidget Origin { get; }
        /// <summary>
        /// Icon that represents this widget.
        /// </summary>
        public Bitmap Icon { get; }
        /// <summary>
        /// Method used to draw the icon.
        /// </summary>
        public ICON_STYLE IconStyle { get; }
        /// <summary>
        /// Duration to display the notification for in milliseconds.
        /// </summary>
        public int DisplayTime { get; }
        /// <summary>
        /// Header text to display at the top of the notification.
        /// </summary>
        public string Header { get; }
        /// <summary>
        /// Main content of the notification.
        /// </summary>
        public string Body { get; }

        /// <summary>
        /// Creates a new Notification.
        /// </summary>
        /// <param name="body">Main content of the notification.</param>
        /// <param name="origin">Widget that sent this notification.</param>
        /// <param name="icon">Icon that represents this widget.</param>
        /// <param name="iconStyle">Method used to draw the icon.</param>
        /// <param name="displayTime">Duration to display the notification for in milliseconds.</param>
        /// <param name="header">Header text to display at the top of the notification.</param>
        public Notification(string body, BaseNotifyingWidget origin = default, Bitmap icon = default, ICON_STYLE iconStyle = default, int displayTime = DEFAULT_DISPLAY_TIME_MS, string header = default)
        {
            Body = body;
            Origin = origin;
            Icon = icon;
            IconStyle = iconStyle;
            DisplayTime = displayTime;
            Header = header;
        }
    }
    }
