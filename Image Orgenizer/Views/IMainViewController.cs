using System.ComponentModel;
using ImageOrganizer.BL;
using Tauron.Application;

namespace ImageOrganizer.Views
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
        void RefreshAll(ProfileData data, string profileName);
        void Closing();
        string GetCurrentImageName();

        void EnterView();
        void ExitView();
    }
}