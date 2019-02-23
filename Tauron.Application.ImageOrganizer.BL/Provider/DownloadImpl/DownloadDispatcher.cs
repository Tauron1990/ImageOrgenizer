using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Tauron.Application.ImageOrganizer.BL.Services;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.BL.Provider.DownloadImpl
{
    public class DownloadDispatcher : IDownloadDispatcher
    {
        private readonly object _lock = new object();
        private readonly IDownloadService _operator;
        private readonly Action<DownloadChangedEventArgs> _onChanged;
        private readonly ConcurrentBag<DownloadEntry> _downloadEntries = new ConcurrentBag<DownloadEntry>();

        public DownloadDispatcher(IDownloadService op, Action<DownloadChangedEventArgs> onChanged)
        {
            _operator = op;
            _onChanged = onChanged;
        }

        public void Dispatch()
        {
            lock (_lock)
            {
                List<(ImageData Data, DownloadItem Item)> toUpdate = new List<(ImageData, DownloadItem)>();
                List<ImageData> toUpdate2 = new List<ImageData>();

                List<DownloadItem> toSchedule = new List<DownloadItem>();

                while (_downloadEntries.TryTake(out var downloadEntry))
                {
                    if (downloadEntry.Failed)
                    {
                        downloadEntry.Item.FailedReason = downloadEntry.FailedReason;
                        _operator.DownloadFailed(downloadEntry.Item);
                        continue;
                    }
                    if (downloadEntry.File.Name != null)
                    {
                        if (!_operator.AddFile(new AddFileInput(downloadEntry.File.Name, downloadEntry.File.Data)))
                        {
                            _onChanged.Invoke(new DownloadChangedEventArgs(DownloadAction.DownloadFailed, downloadEntry.Item));
                            _operator.DownloadFailed(downloadEntry.Item);
                            continue;
                        }
                    }

                    if(downloadEntry.Changed)
                        toUpdate.Add((downloadEntry.Data, downloadEntry.Item));
                    if (downloadEntry.Update)
                    {
                        var item = new DownloadItem(DownloadType.UpdateTags, downloadEntry.Item.Image, DateTime.Now + TimeSpan.FromDays(30), -1, DownloadStade.Queued,
                            downloadEntry.Data.ProviderName, false, null, null);
                        toSchedule.Add(item);
                        _onChanged.Invoke(new DownloadChangedEventArgs(DownloadAction.DownloadAdded, item));
                    }

                    _onChanged(new DownloadChangedEventArgs(DownloadAction.DownloadCompled, downloadEntry.Item));
                    _operator.DownloadCompled(downloadEntry.Item);

                }

                if (toUpdate.Count != 0)
                {
                    toUpdate.ForEach(i =>
                    {
                        switch (i.Item.DownloadType)
                        {
                            case DownloadType.UpdateColor:
                                //FirstOrDefaultAction(i.Data.Tags.Select(td => td.Type), ttd => ttd.Name == i.Item.Metadata, ttd => _operator.UpdateTagType(ttd));
                                _operator.UpdateTagType(i.Data.Tags.Select(td => td.Type).ToArray());
                                break;
                            case DownloadType.UpdateDescription:
                                FirstOrDefaultAction(i.Data.Tags, tag => tag.Name == i.Item.Metadata, tag => _operator.UpdateTag(new UpdateTagInput(tag, true)));
                                break;
                            default:
                                toUpdate2.Add(i.Data);
                                break;
                        }
                    });
                }

                _operator.UpdateImage(toUpdate2.ToArray());
                if (toSchedule.Count != 0)
                    _operator.ScheduleDownload(toSchedule.ToArray());
            }
        }

        public IDownloadEntry Get(DownloadItem item)
        {
            lock (_lock)
            {
                var image = item.DownloadType == DownloadType.DownloadImage ? new ImageData(item.Image, item.Provider) : _operator.GetImageData(item.Image);
                if (image == null)
                {
                    _operator.DownloadCompled(item);
                    return null;
                }
                var dowload = new DownloadEntry(image, item, str => _operator.GetTag(str));
                _downloadEntries.Add(dowload);
                return dowload;
            }
        }

        private static void FirstOrDefaultAction<TType>(IEnumerable<TType> enumerable, Func<TType, bool> predicate, Action<TType> action)
        {
            var value = enumerable.FirstOrDefault(predicate);
            if(value == null) return;

            action(value);
        }
    }
}