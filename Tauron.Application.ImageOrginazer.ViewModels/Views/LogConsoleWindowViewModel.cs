using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [ExportViewModel(AppConststands.LogConsoleWindowName)]
    public class LogConsoleWindowViewModel : ViewModelBase
    {
        [InjectModel(AppConststands.LogConsoleWindowName)]
        public LogConsoleModel ConsoleModel { get; set; }

        public bool Trance
        {
            get => GetProperty<bool>();
            set => SetProperty(value, () =>
            {
                if (value)
                    ConsoleModel.SetFlag(LogLevelFilter.Trance);
                else
                    ConsoleModel.RemoveFlag(LogLevelFilter.Trance);
            });
        }

        public bool Info
        {
            get => GetProperty<bool>();
            set => SetProperty(value, () =>
            {
                if (value)
                    ConsoleModel.SetFlag(LogLevelFilter.Info);
                else
                    ConsoleModel.RemoveFlag(LogLevelFilter.Info);
            });
        }

        public bool Warn
        {
            get => GetProperty<bool>();
            set => SetProperty(value, () =>
            {
                if (value)
                    ConsoleModel.SetFlag(LogLevelFilter.Warn);
                else
                    ConsoleModel.RemoveFlag(LogLevelFilter.Warn);
            });
        }

        public bool Debug
        {
            get => GetProperty<bool>();
            set => SetProperty(value, () =>
            {
                if (value)
                    ConsoleModel.SetFlag(LogLevelFilter.Debug);
                else
                    ConsoleModel.RemoveFlag(LogLevelFilter.Debug);
            });
        }

        public bool Error
        {
            get => GetProperty<bool>();
            set => SetProperty(value, () =>
            {
                if (value)
                    ConsoleModel.SetFlag(LogLevelFilter.Error);
                else
                    ConsoleModel.RemoveFlag(LogLevelFilter.Error);
            });
        }

        public UIObservableCollection<string> Lines { get; set; }

        public override void BuildCompled()
        {
            Trance = true;
            Info = true;
            Warn = true;
            Debug = true;
            Error = true;

            Lines = ConsoleModel.TerminalLines;

            base.BuildCompled();
        }
    }
}