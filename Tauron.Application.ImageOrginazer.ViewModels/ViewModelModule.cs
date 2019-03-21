using System;
using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrganizer.Core;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels
{
    [ExportModule]
    public class ViewModelModule : IModule
    {
        [Inject]
        public ISettingsManager SettingsManager { private get; set; }

        [InjectModel(AppConststands.DownloadManagerModel)]
        public DownloadManagerModel DownloadManagerModel { private get; set; }

        [Inject(typeof(ViewModelBase), ContractName = AppConststands.MainWindowName)]
        public MainWindowViewModel WindowViewModel { get; set; }

        [InjectModel(AppConststands.LogConsoleWindowName)]
        public LogConsoleModel ConsoleModel { get; set; }

        public int Order { get; } = 0;

        public void Initialize(CommonApplication application, Action<ComponentUpdate> addComponent)
        {
            ConsoleModel.Initialize();

            addComponent(new ComponentUpdate("Load Settings"));
            //#if DEBUG
            //SettingsManager.Load("Debug");
            //#else
            SettingsManager.Load(Environment.UserName);
            //#endif

            addComponent(new ComponentUpdate("Start Download Manager"));
            DownloadManagerModel.StartClipBoardListening();

            addComponent(new ComponentUpdate("Load Profile"));
            WindowViewModel.RefreshAll();
        }
    }
}