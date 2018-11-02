using ImageOrganizer.BL;
using Tauron.Application;
using Tauron.Application.Models;

namespace ImageOrganizer.Views
{
    public abstract class MainViewControllerBase : ViewModelBase, IMainViewController
    {
        public virtual UIObservableCollection<TagElement> Tags { get; }
        public virtual UIObservableCollection<TagFilterElement> NavigatorItems { get; }
        public virtual string NavigatorText { get; set; }
        public abstract string ProgrammTitle { get; }
        public virtual bool ImageMenuEnabeld { get; set; }
        public abstract bool IsSidebarEnabled { get; }
        public abstract bool IsNavigatorEnabled { get; }
        public abstract bool IsMainControlEnabled { get; }
        public abstract string ControlButtonLabel { get; }
        public abstract void OnClick();
        public virtual bool CanCreateProfile() => false;

        public virtual void Next()
        {
        }

        public virtual void Back()
        {
        }

        public virtual void RefreshNavigatorText()
        {
        }

        public virtual void RefreshNavigatorItems()
        {
        }

        public virtual void RefreshAll(ProfileData data, string profileName)
        {
        }

        public virtual void Closing()
        {
        }

        public virtual string GetCurrentImageName()
        {
            return null;
        }

        public virtual void EnterView()
        {
            
        }

        public virtual void ExitView()
        {
        }
    }
}