using System.Diagnostics;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.UI;
using Tauron.Application.Models;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views
{
    [DebuggerStepThrough]
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

        public virtual bool CanNext() => true;

        public virtual void Back()
        {
        }

        public virtual bool CanBack() => true;

        public virtual void RefreshNavigatorText()
        {
        }

        public virtual void RefreshNavigatorItems()
        {
        }

        public virtual void RefreshAll(ProfileData data, string profileName, bool valid)
        {
        }

        public virtual void Closing()
        {
        }

        public virtual string GetCurrentImageName() => null;

        public virtual void PrepareDeleteImage()
        {
        }

        public virtual void EnterView()
        {
            
        }

        public virtual void ExitView()
        {
        }
    }
}