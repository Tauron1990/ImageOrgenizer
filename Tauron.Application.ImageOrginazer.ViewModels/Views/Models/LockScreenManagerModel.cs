using System;
using System.Threading;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models
{
    [ExportModel(AppConststands.LockScreenModel)]
    public class LockScreenManagerModel : ModelBase
    {
        private Timer _timer;

        public LockScreenManagerModel() => _timer = new Timer((LockEventHandler));

        private void LockEventHandler(object state) => OnLockEvent();

        private void OnLockEvent() => LockEvent?.Invoke();

        public event Action LockEvent;

        public void Stop() => _timer.Change(TimeSpan.FromDays(2), Timeout.InfiniteTimeSpan);

        public void OnLockscreenReset() => _timer.Change(TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan);
    }
}