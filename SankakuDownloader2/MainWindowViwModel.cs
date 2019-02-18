using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SankakuDownloader2.Core;

namespace SankakuDownloader2
{
    public class MainWindowViwModel : INotifyPropertyChanged
    {
        private bool _setupRunning;

        public MainWindowViwModel()
        {
        }

        public bool SetupRunning
        {
            get => _setupRunning;
            set
            {
                if (value == _setupRunning) return;
                _setupRunning = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}