using System.Collections.Generic;
using System.Linq;
using NLog;
using NLog.Layouts;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models
{
    [ExportModel(AppConststands.LogConsoleWindowName)]
    public class LogConsoleModel : ModelBase
    {
        private class LimitUICollection : UIObservableCollection<string>
        {
            protected override void InsertItem(int index, string item)
            {
                base.InsertItem(index, item);
                CleanUp();
            }

            protected override void SetItem(int index, string item)
            {
                base.SetItem(index, item);
                CleanUp();
            }

            private void CleanUp()
            {
                while (Count > 100) Items.RemoveAt(0);
            }
        }

        private static readonly Dictionary<LogLevel, LogLevelFilter> LogLevelFiltersDic = new Dictionary<LogLevel, LogLevelFilter>
            {
                {LogLevel.Off, LogLevelFilter.None },
                {LogLevel.Trace, LogLevelFilter.Trance },
                {LogLevel.Info, LogLevelFilter.Info },
                {LogLevel.Warn, LogLevelFilter.Warn },
                {LogLevel.Debug, LogLevelFilter.Debug },
                {LogLevel.Error, LogLevelFilter.Error }
            };

        private LimitedList<LogEventInfo> _eventInfos = new LimitedList<LogEventInfo>(100);
        private LogLevelFilter _levelFilter = 0;
        private Layout _layout = "${level} | ${logger} | ${message}";

        public UIObservableCollection<string> TerminalLines { get; } = new LimitUICollection();

        public void SetFlag(LogLevelFilter filter)
        {
            if(_levelFilter.HasFlag(filter)) return;

            _levelFilter |= filter;
            TriggerFilter();
        }

        public void RemoveFlag(LogLevelFilter filter)
        {
            if (!_levelFilter.HasFlag(filter)) return;

            _levelFilter &= ~filter;
            TriggerFilter();
        }

        private void TriggerFilter()
        {
            TerminalLines.Clear();

            TerminalLines.AddRange(_eventInfos.Where(CanAdd).Select(Format));
        }

        public void Initialize() 
            => EventAggregator.Aggregator.GetEvent<NUILogEvent, LogEventInfo>().Subscribe(Hendler);

        private void Hendler(LogEventInfo obj)
        {
            _eventInfos.Add(obj);
            if(CanAdd(obj))
                TerminalLines.Add(Format(obj));
        }

        private bool CanAdd(LogEventInfo info) 
            => LogLevelFiltersDic.TryGetValue(info.Level, out var flag) && _levelFilter.HasFlag(flag);

        private string Format(LogEventInfo info) => _layout.Render(info);
    }
}