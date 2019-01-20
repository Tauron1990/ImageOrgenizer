using Tauron.Application.Views;

namespace Tauron.Application.ImageOrganizer.Views
{
    /// <summary>
    /// Interaktionslogik für ProfileManagerView.xaml
    /// </summary>
    [ExportView(AppConststands.ProfileManagerViewModelName)]
    public partial class ProfileManagerView
    {
        public ProfileManagerView()
        {
            InitializeComponent();
        }
    }
}
