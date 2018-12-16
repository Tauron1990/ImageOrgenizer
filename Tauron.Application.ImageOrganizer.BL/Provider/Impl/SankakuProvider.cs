using System;
using System.Diagnostics;
using System.Linq;
using Tauron.Application.ImageOrganizer.BL.Resources;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Provider.Impl
{
    [Export(typeof(IProvider))]
    public class SankakuProvider : IProvider
    {
        [ThreadStatic]
        private static SankakuBaseProvider _baseProvider;

        private SankakuBaseProvider BaseProvider => _baseProvider ?? (_baseProvider = new SankakuBaseProvider());

        public string Id { get; } = "Provider_Sankaku";

        public bool IsValid(string file) => BaseProvider.IsValidFile(file);
        public bool IsValidUrl(string url)
        {
            Uri uri = new Uri(url);
            return uri.Host.Contains("chan.sankakucomplex.com");
        }

        public void FillInfo(IDownloadEntry entry, Action<string, DownloadType> addDownloadAction)
        {
            var image = entry.Data;
            var item = entry.Item;

            if (!BaseProvider.IsValidFile(image.Name))
            {
                if (item.DownloadType == DownloadType.DownloadImage && image.Name.Contains(@"chan.sankakucomplex.com"))
                    BaseProvider.Load(image.Name);
                else
                {
                    entry.MarkFailed(BuissinesLayerResources.SankakuProvider_InvalidFile);
                    return;
                }
            }
            else
                BaseProvider.LoadPost(image.Name.GetFileNameWithoutExtension());

            if (!BaseProvider.CanRead())
            {
                entry.MarkFailed(BuissinesLayerResources.Sankaku_NotReadable);
                return;
            }
            
            switch (item.DownloadType)
            {
                case DownloadType.UpdateTags:
                    UpdateTags(entry, addDownloadAction);
                    break;
                case DownloadType.DownloadTags:
                    UpdateData(entry);
                    UpdateTags(entry, addDownloadAction);
                    break;
                case DownloadType.DownloadImage:
                    AppConststands.NotImplemented();
                    //bool ok = DownloadImage(entry);
                    //if (!ok)
                    //{
                    //    entry.MarkFailed();
                    //    return;
                    //}
                    //UpdateData(entry);
                    //UpdateTags(entry);
                    break;
                case DownloadType.ReDownload:
                    AppConststands.NotImplemented();
                    entry.MarkFailed(new NotImplementedException());
                    //ok = DownloadImage(entry);
                    //if (!ok)
                    //{
                    //    entry.MarkFailed();
                    //    return;
                    //}
                    break;
                case DownloadType.UpdateColor:
                    string color = null;

                    foreach (var tagTypeData in entry.Data.Tags.Select(t => t.Type).Where(ttd => ttd.Name == entry.Item.Metadata))
                    {
                        if(tagTypeData.Color.StartsWith("#"))
                            continue;

                        if (string.IsNullOrEmpty(color))
                        {
                            color = BaseProvider.GetTagColor(tagTypeData.Name, tagTypeData.Color, out var ok);
                            if (!ok)
                            {
                                entry.MarkFailed(color);
                                return;
                            }
                        }

                        tagTypeData.Color = color;
                        entry.MarkChanged();
                    }

                    break;
                case DownloadType.UpdateDescription:

                    string description = null;

                    (string Description, bool Ok) GetDescription()
                    {
                        BaseProvider.LoadWiki(entry.Item.Metadata);

                        if (!BaseProvider.CanRead())
                        {
                            entry.MarkFailed(BuissinesLayerResources.Sankaku_NotReadable);
                            return (null, false);
                        }

                        var desc = BaseProvider.GetTagDescription(entry.Item.Metadata, out var descok);

                        return (desc, descok);
                    }

                    foreach (var tagData in entry.Data.Tags.Where(t => t.Name == entry.Item.Metadata))
                    {
                        if (!string.IsNullOrWhiteSpace(tagData.Description))
                            continue;

                        if (description == null)
                        {
                            var erg = GetDescription();
                            if (erg.Ok)
                                description = erg.Description;
                            else
                            {
                                entry.MarkFailed(erg.Description);
                                break;
                            }
                        }

                        tagData.Description = description;
                        entry.MarkChanged();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry.Item.DownloadType), entry.Item.DownloadType, null);
            }

            if (NeedUpdate(image) && entry.Item.DownloadType != DownloadType.UpdateDescription && entry.Item.DownloadType != DownloadType.UpdateColor)
                entry.NeedUpdate();
        }

        public void ShowUrl(string name)
        {
            string url = $"https://chan.sankakucomplex.com/post/show/{name}";

            Process.Start(url);
        }

        private bool NeedUpdate(ImageData data) => data.Added + TimeSpan.FromDays(180) > DateTime.Now;

        private void UpdateData(IDownloadEntry downloadEntry)
        {
            var data = downloadEntry.Data;
            var newAdded = BaseProvider.GetDateAdded();
            var newAuthor = BaseProvider.GetAuthor();

            if (data.Added != newAdded)
            {
                data.Added = newAdded;
                downloadEntry.MarkChanged();
            }

            if (data.Author == newAuthor) return;

            data.Author = newAuthor;
            downloadEntry.MarkChanged();

        }

        private void UpdateTags(IDownloadEntry entry, Action<string, DownloadType> addDownloadAction)
        {
            var data = entry.Data;

            foreach (var tag in BaseProvider.GetTags())
            {
                var tagData = data.Tags.FirstOrDefault(td => td.Name == tag.Name);
                if (tagData != null)
                    continue;

                tagData = entry.GetTag(tag.Name);
                if (tagData != null)
                {
                    data.Tags.Add(tagData);
                    entry.MarkChanged();
                    continue;
                }

                tagData = new TagData(new TagTypeData(tag.Type, BaseProvider.GetCssUrl()), string.Empty, tag.Name);

                addDownloadAction(tag.Name, DownloadType.UpdateDescription);
                addDownloadAction(tag.Type, DownloadType.UpdateColor);

                data.Tags.Add(tagData);
                entry.MarkChanged();
            }
        }

        //private bool DownloadImage(IDownloadEntry entry)
        //{
        //    string name = BaseProvider.GetName();
        //    long size = BaseProvider.GetSize();
        //    byte[] bytes = BaseProvider.DownloadImage();

        //    if (size != bytes.Length)
        //        return false;

        //    if (entry.Data.Name != name)
        //    {
        //        entry.Data.Name = name;
        //        entry.MarkChanged();
        //    }
        //    entry.AddFile(name, bytes);
        //    return true;
        //}
    }
}