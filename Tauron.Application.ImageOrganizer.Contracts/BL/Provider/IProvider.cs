using System;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.BL.Provider
{
    public interface IProvider
    {
        string Id { get; }

        bool IsValid(string file);
        bool IsValidUrl(string url);

        void FillInfo(IDownloadEntry entry, Action<string, DownloadType> addDownloadAction);
        void ShowUrl(string name);
    }
}