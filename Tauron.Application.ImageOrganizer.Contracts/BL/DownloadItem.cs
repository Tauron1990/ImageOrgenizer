using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.BL
{
    public class DownloadItem : IEquatable<DownloadItem>, INotifyPropertyChanged
    {
        private DownloadStade _downloadStade;
        private string _failedReason;

        public int Id { get; }

        public DownloadType DownloadType { get; }

        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public DownloadStade DownloadStade
        {
            get => _downloadStade;
            set
            {
                if (value == _downloadStade) return;
                _downloadStade = value;
                OnPropertyChanged();
            }
        }

        public string Provider { get; }

        public string Image { get; }

        public DateTime Schedule { get; }

        public bool RemoveImageOnFail { get; }

        public bool AvoidDouble { get; set; }

        public string FailedReason
        {
            get => _failedReason;
            set
            {
                if (value == _failedReason) return;
                _failedReason = value;
                OnPropertyChanged();
            }
        }

        public string Metadata { get; }

        public DownloadItem(DownloadType downloadType, string image, DateTime schedule, int id, DownloadStade downloadStade, string provider, bool removeImageOnFail, string failedReason, string metadata)
        {
            FailedReason = failedReason;
            Metadata = metadata;
            DownloadType = downloadType;
            Image = image;
            Schedule = schedule;
            Id = id;
            DownloadStade = downloadStade;
            Provider = provider;
            RemoveImageOnFail = removeImageOnFail;
        }

        public DownloadItem(DownloadEntity downloadEntity)
            : this(downloadEntity.DownloadType, downloadEntity.Image, downloadEntity.Schedule, downloadEntity.Id, downloadEntity.DownloadStade,
                downloadEntity.Provider, downloadEntity.RemoveImageOnFail, downloadEntity.FailedReason, downloadEntity.Metadata)
        {
            
        }

        public static bool operator ==(DownloadItem one, DownloadItem two) => one?.Id == two?.Id;

        public static bool operator !=(DownloadItem one, DownloadItem two) => one?.Id != two?.Id;

        public bool Equals(DownloadItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((DownloadItem) obj);
        }

        public override int GetHashCode() => Id;

        public override string ToString() => $"Image={Image}--Date={Schedule}";
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}