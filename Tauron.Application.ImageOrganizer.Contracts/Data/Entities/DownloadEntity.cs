using System;
using System.Diagnostics;
using Tauron.Application.Common.BaseLayer.Data;

namespace Tauron.Application.ImageOrganizer.Data.Entities
{
    [DebuggerDisplay("Image={Image}-Stade={DownloadStade}")]
    public class DownloadEntity : GenericBaseEntity<int>
    {
        private string _image;
        private DownloadType _downloadType;
        private DateTime _schedule;
        private DownloadStade _downloadStade;
        private int _failedCount;
        private string _provider;
        private bool _removeImageOnFail;

        public string Image
        {
            get => _image;
            set => SetWithNotify(ref _image, value);
        }

        public DownloadType DownloadType
        {
            get => _downloadType;
            set => SetWithNotify(ref _downloadType, value);
        }

        public DateTime Schedule
        {
            get => _schedule;
            set => SetWithNotify(ref _schedule, value);
        }

        public DownloadStade DownloadStade
        {
            get => _downloadStade;
            set => SetWithNotify(ref _downloadStade, value);
        }

        public int FailedCount
        {
            get => _failedCount;
            set => SetWithNotify(ref _failedCount, value);
        }

        public string Provider
        {
            get => _provider;
            set => SetWithNotify(ref _provider, value);
        }

        public bool RemoveImageOnFail
        {
            get => _removeImageOnFail;
            set => SetWithNotify(ref _removeImageOnFail, value);
        }
    }
}