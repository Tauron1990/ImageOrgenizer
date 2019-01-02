using System.ComponentModel;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.UI;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    public interface IMainViewController : INotifyPropertyChanged
    {
        UIObservableCollection<TagElement> Tags { get; }
        UIObservableCollection<TagFilterElement> NavigatorItems { get; }
        string NavigatorText { get; set; }
        string ProgrammTitle { get; }

        bool ImageMenuEnabeld { get; set; }

        bool IsSidebarEnabled { get; }
        bool IsNavigatorEnabled { get; }
        bool IsMainControlEnabled { get; }

        string ControlButtonLabel { get; }
        void OnClick();
        bool CanCreateProfile();

        void Next();
        void Back();

        void RefreshNavigatorText();
        void RefreshNavigatorItems();
        void RefreshAll(ProfileData data, string profileName, bool validDatabase);
        void Closing();
        string GetCurrentImageName();
        void PrepareDeleteImage();

        void EnterView();
        void ExitView();
    }
}