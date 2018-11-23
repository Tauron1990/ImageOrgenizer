using Tauron.Application.ImageOrganizer;
using Tauron.Application.ImageOrginazer.ViewModels.Views.Models;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [ExportViewModel(AppConststands.ImageFullScreen)]
    public class ImageFullScreenWindowModel : ViewModelBase
    {
        [InjectModel(AppConststands.FullScreenModelName)]
        public FullScreenModel FullScreenModel { get; set; }
    }
}