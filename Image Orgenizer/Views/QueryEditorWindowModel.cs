using Tauron.Application;
using Tauron.Application.Ioc;
using Tauron.Application.Models;

namespace ImageOrganizer.Views
{
    [ExportViewModel(AppConststands.QueryEditorName)]
    public class QueryEditorWindowModel : ViewModelBase, IResultProvider
    {
        private string _sqlText;
        public object Result => SqlText;

        public string SqlText
        {
            get => _sqlText;
            set => SetProperty(ref _sqlText, value);
        }

        [Inject]
        public QueryEditorWindowModel([Inject]string text) => SqlText = text;

        public QueryEditorWindowModel()
        {
            
        }
    }
}