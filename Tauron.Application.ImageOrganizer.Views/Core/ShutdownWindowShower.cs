using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.Views.Core
{
    [Export(typeof(IShutdownWindowShower))]
    public class ShutdownWindowShower : IShutdownWindowShower
    {
        public void Show() => new ShutdownWindow().Show();
    }
}