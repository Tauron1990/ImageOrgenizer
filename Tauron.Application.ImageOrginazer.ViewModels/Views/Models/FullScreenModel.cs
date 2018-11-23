using Tauron.Application.ImageOrganizer;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models
{
    [ExportModel(AppConststands.FullScreenModelName)]
    public sealed class FullScreenModel : ModelBase
    {
        private object _image;

        public object Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
    }
}