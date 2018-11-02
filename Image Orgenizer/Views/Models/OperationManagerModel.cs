using System;
using System.Threading;
using System.Windows.Input;
using Tauron.Application;
using Tauron.Application.Models;

namespace ImageOrganizer.Views.Models
{
    [ExportModel(AppConststands.OptrationManagerModel)]
    public sealed class OperationManagerModel : ModelBase
    {
        public class NullDispose : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private bool _canPause;
        private bool _canStop;
        private bool _isIntermediate;
        private object _lock = new object();
        private int _maximum;
        private int _operationDepth;
        private bool _operationRunning;
        private Action _pause;
        private string _progressMessage;
        private bool _showMessage;
        private bool _showProgress;
        private Action _stop;
        private int _value;

        public bool ShowMessage
        {
            get => _showMessage;
            set => SetProperty(ref _showMessage, value);
        }

        public bool ShowProgress
        {
            get => _showProgress;
            set => SetProperty(ref _showProgress, value);
        }

        public bool CanPause
        {
            get => _canPause;
            set => SetProperty(ref _canPause, value);
        }

        public bool CanStop
        {
            get => _canStop;
            set => SetProperty(ref _canStop, value);
        }

        public string ProgressMessage
        {
            get => _progressMessage;
            set => SetProperty(ref _progressMessage, value);
        }

        public int Maximum
        {
            get => _maximum;
            set => SetProperty(ref _maximum, value);
        }

        public int Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public bool IsIntermediate
        {
            get => _isIntermediate;
            set => SetProperty(ref _isIntermediate, value);
        }

        public bool OperationRunning
        {
            get => _operationRunning;
            set => SetProperty(ref _operationRunning, value);
        }

        public IDisposable EnterOperation(bool showMessage = false, bool showProgress = false, Action pause = null, Action stop = null)
        {
            Interlocked.Increment(ref _operationDepth);

            if (_operationDepth <= 0 || OperationRunning)
                return new NullDispose();

            lock (_lock)
            {
                ProgressMessage = string.Empty;
                ShowMessage = showMessage;

                Maximum = 100;
                Value = 100;
                ShowProgress = showProgress;

                CanStop = stop != null;
                CanPause = pause != null;

                _pause = pause;
                _stop = stop;

                OperationRunning = true;
            }

            return new ActionDispose(ExitOperation);
        }

        public void PostMessage(string message, int value, int maximum, bool isIntermediate = false)
        {
            ProgressMessage = message;
            Maximum = maximum;
            Value = value;
            IsIntermediate = isIntermediate;
        }

        public void Pause()
        {
            _pause?.Invoke();
        }

        public void Stop()
        {
            _stop?.Invoke();
        }

        private void ExitOperation()
        {
            Interlocked.Decrement(ref _operationDepth);

            if (_operationDepth > 0 || !OperationRunning) throw new NotSupportedException();
            lock (_lock)
            {
                _pause = null;
                _stop = null;
                OperationRunning = false;
            }
        }
    }

    public class ActionDispose : IDisposable
    {
        private readonly Action _action;

        public ActionDispose(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            _action();
            UiSynchronize.Synchronize.BeginInvoke(CommandManager.InvalidateRequerySuggested);
        }
    }
}