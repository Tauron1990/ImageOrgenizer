using System;
using System.Collections.Concurrent;
using System.Threading;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;
using Tauron.Application.Ioc;

namespace ImageOrganizer.Startup.BrowserImpl
{
    [Export(typeof(IBrowserManager))]
    public class BrowserManager : IBrowserManager, IDisposable
    {
        private readonly ConcurrentDictionary<int, BrowserImpl> _browser = new ConcurrentDictionary<int, BrowserImpl>();

        public IBrowserHelper GetBrowser() => _browser.GetOrAdd(Thread.CurrentThread.ManagedThreadId, i => new BrowserImpl());

        public void Dispose()
        {
            foreach (var impl in _browser) impl.Value.Dispose();
        }
    }
}