using System;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Provider.Impl
{
    [Export(typeof(IProvider))]
    public class NonProvider : IProvider
    {
        public string Id { get; } = nameof(AppConststands.ProviderNon);


        public bool IsValid(string file) => true;
        public bool IsValidUrl(string url) => false;

        public void FillInfo(IDownloadEntry entry, Action<string, DownloadType> addDownloadAction)
        {
            if (entry.Item.DownloadType == DownloadType.DownloadImage)
                TryAddFile(entry);
        }

        public void ShowUrl(string name) => AppConststands.NotImplemented();

        private void TryAddFile(IDownloadEntry entry)
        {
            var image = entry.Data;

            if (!image.Name.ExisFile()) return;

            string name = image.Name.GetFileName();

            entry.AddFile(name, image.Name.ReadAllBytes());
            image.Name = name;
        }
    }
}