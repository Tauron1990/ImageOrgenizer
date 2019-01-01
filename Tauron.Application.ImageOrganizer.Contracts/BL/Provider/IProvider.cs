using System;
using Tauron.Application.ImageOrganizer.BL.Provider.Browser;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.BL.Provider
{
    public interface IProvider
    {
        string Id { get; }

        string NameFromUrl(string url);
        bool IsValid(string file);
        bool IsValidUrl(string url);

        void FillInfo(IDownloadEntry entry, IBrowserHelper browser, Action<string> delay, Action<string, DownloadType> addDownloadAction);
        void ShowUrl(string name);
    }
}