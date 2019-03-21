using NLog;
using NLog.Targets;
using Tauron.Application;
using Tauron.Application.ImageOrganizer.UI;

namespace ImageOrganizer
{
    public class UILayout : Target 
    {
        protected override void Write(LogEventInfo logEvent) 
            => EventAggregator.Aggregator.GetEvent<NUILogEvent, LogEventInfo>().Publish(logEvent);
    }
}