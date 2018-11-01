using System;
using System.Threading.Tasks;
using ImageOrganizer.BL;
using ImageOrganizer.Resources;
using ImageOrganizer.Views.Models;
using Tauron.Application;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace ImageOrganizer.Views.Controls
{
    [ExportViewModel(AppConststands.CustomQueryControl)]
    public class CustomQueryViewModel : ViewModelBase
    {
        private string _sqlText;

        [Inject]
        public Operator Operator { get; set; }

        [InjectModel(AppConststands.OptrationManagerModel)]
        public OperationManagerModel OperationManager { get; set; }

        [InjectModel(AppConststands.ImageManagerModel)]
        public ImageViewerModel ViewerModel { get; set; }

        public string SqlText
        {
            get => _sqlText;
            set => SetProperty(ref _sqlText, value);
        }

        public Func<ImageData> GetImageData { get; set; }
        public Action<RawSqlResult> Update { get; set; }

        public event Func<RawSqlResult, bool> ValidateResult; 

        [CommandTarget]
        public void EditQuery()
        {
            var window = ViewManager.CreateWindow(AppConststands.QueryEditorName);
            window.ShowDialogAsync(MainWindow).ContinueWith(t =>
            {
                if (window.DialogResult == true)
                    SqlText = window.Result as string;
            });
        }

        [CommandTarget]
        public void GeneratePreview() => ShowPreview(GetImageData());

        [CommandTarget]
        public bool CanGeneratePreview() => GetImageData?.Invoke() != null;

        [CommandTarget]
        public void StartQuery()
        {
            if (string.IsNullOrWhiteSpace(SqlText)) return;

            using (OperationManager.EnterOperation())
            {
                var result = Operator.ExecuteRawSql(SqlText);

                if (!result.Sucess)
                {
                    if (result.Exception != null)
                        Dialogs.FormatException(MainWindow, result.Exception);
                    else
                        Dialogs.ShowMessageBox(MainWindow, UIResources.ImageViewer_Error_NoData, "Error", MsgBoxButton.Ok, MsgBoxImage.Error, null);
                    return;
                }
                
                ShowPreview(result.ImageData).ContinueWith(t =>
                {
                    if (OnProfileDataCreated(result)) return;

                    //if (Dialogs.ShowMessageBox(MainWindow, UIResources.ProfileManager_OverrideProfile_Text, UIResources.ProfileManager_OverrideProfile_Caption,
                    //        MsgBoxButton.YesNo, MsgBoxImage.Information, null) == MsgBoxResult.Ok)
                    Update(result);
                });
            }
        }

        private Task ShowPreview(ImageData data) => ViewManager.CreateWindow(AppConststands.PreviewWindowName, data).ShowDialogAsync(MainWindow);

        private bool OnProfileDataCreated(RawSqlResult arg) => ValidateResult?.Invoke(arg) ?? false;
    }
}