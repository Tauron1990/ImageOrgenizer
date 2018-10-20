using System.Windows.Media;
using Tauron.Application.Models;

namespace ImageOrganizer.Views.Models
{
    [ExportModel(AppConststands.FullScreenModelName)]
    public sealed class FullScreenModel : ModelBase
    {
        private ImageSource _image;

        public ImageSource Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
    }
}