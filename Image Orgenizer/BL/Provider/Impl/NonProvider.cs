using ImageOrganizer.Data.Entities;
using Tauron;
using Tauron.Application.Ioc;

namespace ImageOrganizer.BL.Provider.Impl
{
    [Export(typeof(IProvider))]
    public class NonProvider : IProvider
    {
        public const string ProviderNon = "ProviderNon";

        public string Id { get; } = nameof(ProviderNon);


        public bool IsValid(string file) => true;
        public bool IsValidUrl(string url) => false;

        public void FillInfo(IDownloadEntry entry)
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