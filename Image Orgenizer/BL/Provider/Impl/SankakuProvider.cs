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

        [Inject]
        public Operator Operator { get; set; }

        public string Id { get; } = "Provider_Sankaku";

        public bool IsValid(string file) => _baseProvider.IsValidFile(file);
        public bool IsValidUrl(string url)
        {
            Uri uri = new Uri(url);
            return uri.Host.Contains("chan.sankakucomplex.com");
        }

        public bool FillInfo(ImageData image, DownloadType downloadItemDownloadType, out bool ok)
        {
            if (!_baseProvider.IsValidFile(image.Name))
            {
                if (downloadItemDownloadType == DownloadType.DownloadImage && image.Name.Contains(@"chan.sankakucomplex.com"))
                    _baseProvider.Load(image.Name);
                else
                {
                    ok = false;
                    return false;
                }
            }
            else
                _baseProvider.LoadPost(image.Name.GetFileNameWithoutExtension());

            if (!_baseProvider.CanRead())
            {
                ok = false;
                return false;
            }

            ok = true;

            switch (downloadItemDownloadType)
            {
                case DownloadType.UpdateTags:
                    UpdateTags(image);
                    break;
                case DownloadType.DownloadTags:
                    UpdateData(image);
                    UpdateTags(image);
                    break;
                case DownloadType.DownloadImage:
                    ok = DownloadImage(image);
                    if (!ok) return false;
                    UpdateData(image);
                    UpdateTags(image);
                    break;
                case DownloadType.ReDownload:
                    ok = DownloadImage(image);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(downloadItemDownloadType), downloadItemDownloadType, null);
            }

            return NeedUpdate(image);
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

        private bool DownloadImage(ImageData data)
        {
            string name = _baseProvider.GetName();
            long size = _baseProvider.GetSize();
            byte[] bytes = _baseProvider.DownloadImage();

            if (size != bytes.Length)
                return false;

            data.Name = name;
            Operator.AddFile(new AddFileInput(name, bytes));
            return true;
        }
    }
}