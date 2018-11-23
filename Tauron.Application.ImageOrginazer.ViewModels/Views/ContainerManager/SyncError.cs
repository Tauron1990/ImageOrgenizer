using System;
using System.Windows.Input;
using Tauron.Application.Commands;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.Container;
using Tauron.Application.ImageOrginazer.ViewModels.Resources;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.ContainerManager
{
    public class SyncError : UiBase
    {
        private readonly IOperator _op;
        private readonly ErrorType _errorType;
        private object _stade;
        private bool _isAdded;

        public SyncError(string name, ErrorType type, IOperator op)
        {
            _op = op;
            _errorType = type;
            Name = name;
            ElementClick = new SimpleCommand(CanClick, Click);

            switch (type)
            {
                case ErrorType.Deleted:
                    Type = UIResources.ContainerManager_ErrorType_Deleted;
                    Stade = new SyncErrorNoStade();
                    break;
                case ErrorType.DataMissing:
                    Type = UIResources.ContainerManager_ErrorType_Missing;
                    Stade = new SyncErrorAddStade();
                    break;
                case ErrorType.ComposeAdd:
                    Type = UIResources.ContainerManager_ErrorType_Add;
                    Stade = new SyncErrorNoStade();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private bool CanClick(object arg) => !_isAdded && _errorType == ErrorType.DataMissing;

        private void Click(object obj)
        {
            _isAdded = true;
            Stade = new SyncErrorProgressStade();

            _op.ScheduleRedownload(Name)
                .ContinueWith(t =>
                {
                    if (t.Exception == null)
                    {
                        if (t.Result)
                            Stade = new SyncErrorCompledStade();
                        else
                            Stade = new SyncErrorErrorStade();
                    }
                    else
                        Stade = new SyncErrorErrorStade();
                });
        }

        public string Name { get; }

        public string Type { get; }

        public object Stade
        {
            get => _stade;
            set => SetProperty(ref _stade, value);
        }

        public ICommand ElementClick { get; set; }
    }
}