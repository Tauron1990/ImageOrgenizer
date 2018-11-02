using System;
using System.Collections.Generic;
using ImageOrganizer.Data.Entities;

namespace ImageOrganizer.BL.Provider.DownloadImpl
{
    public class DownloadDispatcher : IDownloadDispatcher
    {
        private readonly object _lock = new object();
        private readonly Operator _operator;
        private readonly Action<DownloadChangedEventArgs> _onChanged;
        private readonly List<DownloadEntry> _downloadEntries = new List<DownloadEntry>();

        public DownloadDispatcher(Operator op, Action<DownloadChangedEventArgs> onChanged)
        {
            _operator = op;
            _onChanged = onChanged;
        }

        public void Dispatch()
        {
            lock (_lock)
            {
                List<ImageData> toUpdate = new List<ImageData>();
                List<DownloadItem> toSchedule = new List<DownloadItem>();

                foreach (var downloadEntry in _downloadEntries)
                {
                    if (downloadEntry.Failed)
                    {
                        _operator.DownloadFailed(downloadEntry.Item);
                        continue;
                    }
                    if (downloadEntry.File.Name != null)
                    {
                        if (!_operator.AddFile(new AddFileInput(downloadEntry.File.Name, downloadEntry.File.Data)).Result)
                        {
                            _operator.DownloadFailed(downloadEntry.Item);
                            continue;
                        }
                    }

                    if(downloadEntry.Changed)
                        toUpdate.Add(downloadEntry.Data);
                    if (downloadEntry.Update)
                    {
                        var item = new DownloadItem(DownloadType.UpdateTags, downloadEntry.Item.Image, DateTime.Now + TimeSpan.FromDays(30), -1, DownloadStade.Queued,
                            downloadEntry.Data.ProviderName, false);
                        toSchedule.Add(item);
                        _onChanged.Invoke(new DownloadChangedEventArgs(DownloadAction.DownloadAdded, item));
                    }

                    _onChanged(new DownloadChangedEventArgs(DownloadAction.DownloadCompled, downloadEntry.Item));
                    _operator.DownloadCompled(downloadEntry.Item);

                    if(toUpdate.Count != 0)
                        _operator.UpdateImage(toUpdate.ToArray());
                    if (toUpdate.Count != 0)
                        _operator.ScheduleDownload(toSchedule.ToArray());
                }

                _downloadEntries.Clear();
            }
        }

        public IDownloadEntry Get(DownloadItem item)
        {
            lock (_lock)
            {
                var image = item.DownloadType == DownloadType.DownloadImage ? new ImageData(item.Image, item.Provider) : _operator.GetImageData(item.Image).Result;
                if (image == null)
                {
                    _operator.DownloadCompled(item);
                    return null;
                }
                var dowload = new DownloadEntry(image, item);
                _downloadEntries.Add(dowload);
                return dowload;
            }
        }
    }
}