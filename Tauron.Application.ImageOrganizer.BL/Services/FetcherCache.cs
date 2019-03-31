using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Tauron.Application.ImageOrganizer.BL.Provider;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Services
{
    [Export(typeof(IFetcherCache))]
    public class FetcherCache : IFetcherCache, INotifyBuildCompled
    {
        private const string FetcherCacheName = "FetcherCache";

        private sealed class FetcherCacheSettings : TauronProfile
        {
            public FetcherCacheSettings([NotNull] string application, [NotNull] string defaultPath)
                : base(application, defaultPath) { }

            public void Load() => Load(string.Empty);

            public bool Fetching
            {
                get => GetValue(false);
                set => SetVaue(value);
            }

            public int UIPage
            {
                get => GetValue(0);
                set => SetVaue(value);
            }

            public string Last
            {
                get => GetValue(string.Empty);
                set => SetVaue(value);
            }
            public string Next
            {
                get => GetValue(string.Empty);
                set => SetVaue(value);
            }
            public int FetcherPage
            {
                get => GetValue(0);
                set => SetVaue(value);
            }
            public string FetcherId
            {
                get => GetValue(string.Empty);
                set => SetVaue(value);
            }
        }

        private sealed class CacheImage
        {
            public string FilePath { get; set; }

            public string Link { get; set; }

            public string Info { get; set; }
        }

        private sealed class CacheStade
        {
            private const string ImageDb = "Images.db";

            private int _lastImageCount;

            public List<CacheImage> CacheImages { get; set; }

            public FetcherCacheSettings CacheSettings { get; set; }

            public void Read(string path)
            {
                string dbPath = path.CombinePath(ImageDb);
                if(!dbPath.ExisFile()) return;

                CacheSettings.Load();
                CacheImages.Clear();

                using (var stream = new BinaryReader(dbPath.OpenRead()))
                {
                    int count = stream.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        CacheImages.Add(new CacheImage
                        {
                            FilePath = stream.ReadString(),
                            Info = stream.ReadString(),
                            Link = stream.ReadString()
                        });
                    }
                }

                _lastImageCount = CacheImages.Count;
            }

            public void Write(string path)
            {
                CacheSettings.Save();

                if(_lastImageCount == CacheImages.Count) return;

                using (var stream = new BinaryWriter(path.CombinePath(ImageDb).OpenWrite()))
                {
                    stream.Write(CacheImages.Count);

                    foreach (var cacheImage in CacheImages)
                    {
                        stream.Write(cacheImage.FilePath);
                        stream.Write(cacheImage.Info);
                        stream.Write(cacheImage.Link);
                    }
                }

                _lastImageCount = CacheImages.Count;
            }

            public void Clear(string path)
            {
                CacheImages.Clear();
                path.CombinePath(ImageDb).DeleteFile();

                CacheSettings.Clear();
                CacheSettings.Save();
            }
        }

        private string _targetPath;
        private string _imagesLocation;
        private CacheStade _cacheStade;

        public string FetcherId => _cacheStade.CacheSettings.FetcherId;
        public bool Fetching => _cacheStade.CacheSettings.Fetching;
        public int UIPage => _cacheStade.CacheSettings.UIPage;

        public IList<FetcherImage> Images => _cacheStade.CacheImages.Select(image => new FetcherImage
        {
            Image = File.ReadAllBytes(image.FilePath),
            Info = image.Info,
            Link = image.Link

        }).ToList();
        public string Last => _cacheStade.CacheSettings.Last;
        public string Next => _cacheStade.CacheSettings.Next;
        public int FetcherPage => _cacheStade.CacheSettings.FetcherPage;

        public void Clear() => _cacheStade.Clear(_targetPath);

        public void Read() => _cacheStade.Read(_targetPath);

        public void Start(string fetcherId)
        {
            _cacheStade.CacheSettings.FetcherId = fetcherId;
            _cacheStade.CacheSettings.Fetching = true;
            _cacheStade.Write(_targetPath);
        }

        public void Feed(FetcherResult result, string last, int fetcherPage)
        {
            _cacheStade.CacheSettings.Last = last;
            _cacheStade.CacheSettings.FetcherPage = fetcherPage;
            _cacheStade.CacheSettings.Next = result.Next;

            foreach (var fetcherImage in result.Images)
            {
                string targetFile = Path.Combine(_imagesLocation, Guid.NewGuid() + ".bin");
                File.WriteAllBytes(targetFile, fetcherImage.Image);

                _cacheStade.CacheImages.Add(new CacheImage
                {
                    FilePath = targetFile,
                    Info = fetcherImage.Info,
                    Link = fetcherImage.Link
                });
            }

            _cacheStade.Write(_targetPath);
        }

        public void SetUIPage(int page)
        {
            _cacheStade.CacheSettings.UIPage = page;
            _cacheStade.Write(_targetPath);
        }

        public void BuildCompled()
        {
            string basePath = CommonApplication.Current.GetdefaultFileLocation();
            _imagesLocation = basePath.CombinePath(FetcherCacheName, "Files");
            _imagesLocation.CreateDirectoryIfNotExis();
            _cacheStade = new CacheStade
            {
                CacheImages = new List<CacheImage>(),
                CacheSettings = new FetcherCacheSettings(FetcherCacheName, basePath)
            };

            _targetPath = _targetPath.CombinePath(FetcherCacheName);
            _targetPath.CreateDirectoryIfNotExis();
        }
    }
}