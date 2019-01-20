using System;
using Tauron.Application.ImageOrganizer.BL;

namespace Tauron.Application.ImageOrginazer.ViewModels.Views.Models
{
    public class ProfileManagerElement : ObservableObject
    {
        private string _name;
        private ProfileData _data;
        private bool _active;

        public event Action<ProfileManagerElement> Toggle;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ProfileData Data
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }

        public bool Active
        {
            get => _active;
            set => SetProperty(ref _active, value, () => Toggle?.Invoke(this));
        }
    }
}