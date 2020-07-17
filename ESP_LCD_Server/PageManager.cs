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

        public static AbstractPage CurrentPage { get; private set; }
        public static int PageCount => pages.Count;
        public delegate void NotifyEventHandler(AbstractPage page);
        public static event NotifyEventHandler Notify;
        public delegate void PageChangedEventHandler(AbstractPage oldPage, AbstractPage newPage, int pageChangeOffset);
        public static event PageChangedEventHandler PageChanged;

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
        public static AbstractPage GetPage(int pageIndex) => pages[pageIndex];

        private static void OffsetPage(int offset)
        {
            if (CurrentPage == null) return;
            if (offset == 0) return;

            int newPageIndex = (pages.IndexOf(CurrentPage) + offset) % pages.Count;
            if (newPageIndex < 0) newPageIndex += pages.Count;

            AbstractPage oldPage = CurrentPage;
            AbstractPage newPage = pages[newPageIndex];


            PageChangedEventHandler handler = PageChanged;
            handler?.Invoke(oldPage, newPage, offset);

            CurrentPage = newPage;

        }


    }
}
