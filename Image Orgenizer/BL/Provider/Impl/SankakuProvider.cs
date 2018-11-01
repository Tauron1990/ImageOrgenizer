using System;
using System.Linq;
using ImageOrganizer.Data.Entities;
using Tauron;
using Tauron.Application.Ioc;

namespace ImageOrganizer.BL.Provider.Impl
{
    [Export(typeof(IProvider))]
    public class SankakuProvider : IProvider
    {
        private readonly SankakuBaseProvider _baseProvider = new SankakuBaseProvider();

        public string Id { get; } = "Provider_Sankaku";

        public bool IsValid(string file) => _baseProvider.IsValidFile(file);
        public bool IsValidUrl(string url)
        {
            Uri uri = new Uri(url);
            return uri.Host.Contains("chan.sankakucomplex.com");
        }

        public void FillInfo(IDownloadEntry entry)
        {
            var image = entry.Data;
            var item = entry.Item;

            if (!_baseProvider.IsValidFile(image.Name))
            {
                if (item.DownloadType == DownloadType.DownloadImage && image.Name.Contains(@"chan.sankakucomplex.com"))
                    _baseProvider.Load(image.Name);
                else
                {
                    entry.MarkFailed();
                    return;
                }
            }
            else
                _baseProvider.LoadPost(image.Name.GetFileNameWithoutExtension());

            if (!_baseProvider.CanRead())
            {
                entry.MarkFailed();
                return;
            }
            
            switch (item.DownloadType)
            {
                case DownloadType.UpdateTags:
                    UpdateTags(image);
                    break;
                case DownloadType.DownloadTags:
                    UpdateData(image);
                    UpdateTags(image);
                    break;
                case DownloadType.DownloadImage:
                    bool ok = DownloadImage(entry);
                    if (!ok)
                    {
                        entry.MarkFailed();
                        return;
                    }
                    UpdateData(image);
                    UpdateTags(image);
                    break;
                case DownloadType.ReDownload:
                    ok = DownloadImage(entry);
                    if (!ok)
                    {
                        entry.MarkFailed();
                        return;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry.Item.DownloadType), entry.Item.DownloadType, null);
            }

            if (NeedUpdate(image))
                entry.NeedUpdate();
        }

        private bool NeedUpdate(ImageData data) => data.Added + TimeSpan.FromDays(180) > DateTime.Now;

        private void UpdateData(ImageData data)
        {
            data.Added = _baseProvider.GetDateAdded();
            data.Author = _baseProvider.GetAutor();
        }

        private void UpdateTags(ImageData data)
        {
            foreach (var tag in _baseProvider.GetTags())
            {
                var tagData = data.Tags.FirstOrDefault(td => td.Name == tag.Name);
                if (tagData != null)
                    continue;

                tagData = new TagData(new TagTypeData(tag.Type, _baseProvider.GetTagColor(tag.Type)), _baseProvider.GetTagDescription(tag.Name), tag.Name);

                data.Tags.Add(tagData);
            }
        }

        private bool DownloadImage(IDownloadEntry entry)
        {
            string name = _baseProvider.GetName();
            long size = _baseProvider.GetSize();
            byte[] bytes = _baseProvider.DownloadImage();

            if (size != bytes.Length)
                return false;

            entry.Data.Name = name;
            entry.AddFile(name, bytes);
            return true;
        }
    }
}