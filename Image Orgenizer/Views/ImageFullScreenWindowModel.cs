using ImageOrganizer.Views.Models;
using Tauron.Application.Models;

namespace ImageOrganizer.Views
{
    [ExportViewModel(AppConststands.ImageFullScreen)]
    public class ImageFullScreenWindowModel : ViewModelBase
    {
        [InjectModel(AppConststands.FullScreenModelName)]
        public FullScreenModel FullScreenModel { get; set; }
    }
}