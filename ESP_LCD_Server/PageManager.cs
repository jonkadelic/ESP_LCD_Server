using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ESP_LCD_Server
{
    public static class PageManager
    {
        private static List<AbstractPage> pages = new List<AbstractPage>();
        private const int PAGE_REDRAW_INTERVAL_MS = 250;
        public static AbstractPage CurrentPage { get; private set; }
        public delegate void NotifyEventHandler(AbstractPage page);
        public static event NotifyEventHandler Notify;

        public static void Run()
        {
            RenderPagesTaskAsync().ConfigureAwait(false);
        }

        public static void AddPage(AbstractPage page)
        {
            pages.Add(page);
            page.Notify += OnNotify;
            if (CurrentPage == null) CurrentPage = page;
        }

        private static void OnNotify(AbstractPage sender)
        {
            NotifyEventHandler handler = Notify;
            handler?.Invoke(sender);
        }

        public static void NextPage() => OffsetPage(1);
        public static void LastPage() => OffsetPage(-1);
        public static void Action() => CurrentPage.HandleActionAsync().ConfigureAwait(false);

        private static void OffsetPage(int offset)
        {
            if (CurrentPage == null) return;

            CurrentPage = pages[(pages.IndexOf(CurrentPage) + offset) % pages.Count];
        }

        private static async Task RenderPagesTaskAsync()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    foreach (AbstractPage page in pages)
                    {
                        page.RenderFrameAsync();
                    }
                    Thread.Sleep(PAGE_REDRAW_INTERVAL_MS);
                }
            });
        }
    }
}
