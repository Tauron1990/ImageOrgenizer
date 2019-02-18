using Tauron.Application.ImageOrganizer.BL.Provider.Browser;
using Tauron.Application.Ioc;

namespace ImageOrganizer.BrowserImpl
{
    [Export(typeof(IBrowserManager))]
    public class BrowserManager : IBrowserManager
    {
        private IBrowserHelper _browserHelper;

        public IBrowserHelper GetBrowser() => _browserHelper ?? (_browserHelper = new BrowserHelper());
    }
}