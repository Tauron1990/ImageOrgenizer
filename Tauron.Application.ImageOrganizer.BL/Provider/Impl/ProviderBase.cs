using System;
using NLog;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.BL.Provider.Impl
{
    public abstract class ProviderBase : IProvider
    {
        protected Logger Logger { get; }

        protected ProviderBase() => Logger = LogManager.GetLogger(GetType().Name);
        public abstract string Id { get; }
        public abstract string NameFromUrl(string url);
        public abstract bool IsValid(string file);
        public abstract bool IsValidUrl(string url);
        public abstract void FillInfo(IDownloadEntry entry, IBrowserHelper browser, Action<string> delay, Action<string, DownloadType> addDownloadAction);
        public abstract void ShowUrl(string name);
    }
}