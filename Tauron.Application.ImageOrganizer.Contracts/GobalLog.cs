using NLog;

namespace Tauron.Application.ImageOrganizer
{
    public class GlobalLogConststands
    {
        public static readonly Logger PagerLogger = LogManager.GetLogger("Pager");
    }
}