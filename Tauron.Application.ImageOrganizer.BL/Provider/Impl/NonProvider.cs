using System;
using System.IO;
using NLog;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Provider.Impl
{
    [Export(typeof(IProvider))]
    public class NonProvider : ProviderBase
    {
        public override string Id { get; } = nameof(AppConststands.ProviderNon);


        public override string NameFromUrl(string url)
        {
            try
            {
                return Path.GetFileNameWithoutExtension(url) ?? url;
            }
            catch (ArgumentException)
            {
                return url;
            }
        }

        public override bool IsValid(string file) => true;
        public override bool IsValidUrl(string url) => false;

        public override void FillInfo(IDownloadEntry entry, IBrowserHelper browser, Action<string> delay, Action<string, DownloadType> addDownloadAction)
        {
            Logger.Info($"No Infos added to Image: {entry.Data.Name}");

            if (entry.Item.DownloadType == DownloadType.DownloadImage)
                TryAddFile(entry);
        }

        public override void ShowUrl(string name)
        {
            Logger.Warn("Show Url Not Possibly");
            AppConststands.NotImplemented();
        }

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