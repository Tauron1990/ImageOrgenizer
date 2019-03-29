using System;
using System.Diagnostics;
using System.Linq;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;
using Tauron.Application.ImageOrganizer.BL.Resources;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Provider.Impl
{
    [Export(typeof(IProvider))]
    public class SankakuProvider : ProviderBase
    {
        private class SankakuProviderHolder
        {
            public SankakuBaseProvider SankakuBaseProvider { get; } = new SankakuBaseProvider();

            private IDataInterceptor DataInterceptor { get; }

            private byte[] Data { get; set; }

            public bool Intercept { private get; set; }

            public SankakuProviderHolder()
            {
                DataInterceptor = new SankakuDataInterceptor(() => Intercept, bytes => Data = bytes);
            }

            public void Init(IBrowserHelper helper)
            {
                helper.RegisterInterceptor(SanId, () => DataInterceptor);
                SankakuBaseProvider.Init(helper, GetData);
            }

            private byte[] GetData() => Data;

            public void CleanUp()
            {
                Data = null;
                Intercept = false;
            }
        }

        [ThreadStatic]
        private static SankakuProviderHolder _baseProvider;

        private SankakuProviderHolder BaseProviderCore => _baseProvider ?? (_baseProvider = new SankakuProviderHolder());
        private SankakuBaseProvider BaseProvider => BaseProviderCore.SankakuBaseProvider;

        private const string SanId = "Provider_Sankaku";

        public override string Id { get; } = SanId;

        public override string NameFromUrl(string url) => new Uri(url).Segments.Last();

        public override bool IsValid(string file) => BaseProvider.IsValidFile(file);
        public override bool IsValidUrl(string url)
        {
            Uri uri = new Uri(url);
            return uri.Host.Contains("chan.sankakucomplex.com");
        }

        private bool Load(Func<bool> load, Action<string> delay, IDownloadEntry entry)
        {
            var ok = load();
            if (ok) return true;

            delay(Id);
            entry.MarkFailed(BuissinesLayerResources.Sankaku_NotReadable);
            return false;

        }

        public override void FillInfo(IDownloadEntry entry, IBrowserHelper browser,Action<string> delay, Action<string, DownloadType> addDownloadAction)
        {
            try
            {
                Logger.Info($"Download Info from Sankaku: {entry.Data.Name}");

                BaseProviderCore.Init(browser);
                BaseProviderCore.Intercept = entry.Item.DownloadType == DownloadType.DownloadImage || entry.Item.DownloadType == DownloadType.ReDownload;

                var image = entry.Data;
                var item = entry.Item;

                if (entry.Item.DownloadType != DownloadType.UpdateColor && entry.Item.DownloadType != DownloadType.UpdateDescription)
                {
                    if (!BaseProvider.IsValidFile(image.Name))
                    {
                        if (item.DownloadType == DownloadType.DownloadImage && image.Name.Contains(@"chan.sankakucomplex.com"))
                        {
                            if (!Load(() => BaseProvider.Load(image.Name), delay, entry)) return;
                        }
                        else
                        {
                            entry.MarkFailed(BuissinesLayerResources.SankakuProvider_InvalidFile);
                            return;
                        }
                    }
                    else
                    {
                        if(!Load(() => BaseProvider.LoadPost(image.Name.GetFileNameWithoutExtension()), delay, entry)) return;
                    }

                    if (!BaseProvider.CanRead(out var delayBool))
                    {
                        if (delayBool)
                            delay(Id);
                        entry.MarkFailed(BuissinesLayerResources.Sankaku_NotReadable);
                        return;
                    }
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
                        UpdateData(entry);
                        UpdateTags(entry, addDownloadAction);

                        bool ok = DownloadImage(entry, browser, delay);
                        if (!ok)
                        {
                            entry.MarkFailed(BuissinesLayerResources.SankakuProvider_DownlodInvalid);
                            return;
                        }
                        break;
                    case DownloadType.ReDownload:
                        //AppConststands.NotImplemented();
                        //entry.MarkFailed(new NotImplementedException());
                        ok = DownloadImage(entry, browser, delay);
                        if (!ok)
                        {
                            entry.MarkFailed(BuissinesLayerResources.SankakuProvider_DownlodInvalid);
                            return;
                        }
                        break;
                    case DownloadType.UpdateColor:
                        string color = null;

                        foreach (var tagTypeData in entry.Data.Tags.Select(t => t.Type))//.Where(ttd => ttd.Name == entry.Item.Metadata))
                        {
                            if(tagTypeData.Color.StartsWith("#"))
                                continue;

                            if (string.IsNullOrEmpty(color))
                            {
                                color = BaseProvider.GetTagColor(tagTypeData.Name, tagTypeData.Color, out var cok);
                                if (!cok)
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
                            if (!Load(() => BaseProvider.LoadWiki(entry.Item.Metadata), delay, entry))
                                return (null, false);

                            if (!BaseProvider.CanRead(out var delayBool))
                            {
                                if (delayBool)
                                    delay(Id);
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
            finally
            {
                BaseProviderCore.CleanUp();
            }
        }

        public override void ShowUrl(string name)
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

        private bool DownloadImage(IDownloadEntry entry, IBrowserHelper helper, Action<string> delay)
        {
            AppConststands.NotImplemented();
            return false;

            ////string url = BaseProvider.GetDownloadUrl();
            //string name = BaseProvider.GetName();
            //long size = BaseProvider.GetSize();

            ////BaseProviderCore.Intercept = true;
            ////if (!Load(() => helper.Load(url), delay, entry)) return false;

            //byte[] bytes = BaseProvider.DownloadImage();

            //if (size != bytes.Length)
            //    return false;

            //if (entry.Data.Name != name)
            //{
            //    entry.Data.Name = name;
            //    entry.MarkChanged();
            //}
            //entry.AddFile(name, bytes);
            //return true;
        }
    }
}