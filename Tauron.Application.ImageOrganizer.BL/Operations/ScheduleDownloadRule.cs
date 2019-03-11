using System;
using System.Collections.Generic;
using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.BL.Provider;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;
using Tauron.Application.Ioc;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.ScheduleDonwnload)]
    public class ScheduleDownloadRule : IOBusinessRuleBase<DownloadItem[], DownloadItem[]>
    {
        [Inject]
        public IProviderManager ProviderManager { get; set; }

        [InjectRepo]
        public IDownloadRepository DownloadRepository { get; set; }

        [InjectRepo]
        public Lazy<IImageRepository> ImageRepository { get; set; }

        public override DownloadItem[] ActionImpl(DownloadItem[] inputs)
        {
            List<DownloadEntity> items = new List<DownloadEntity>();

            using (var db = Enter())
            {
                foreach (var input in inputs)
                {
                    //if (input.DownloadType == DownloadType.DownloadImage || input.DownloadType == DownloadType.ReDownload)
                    //{
                    //    AppConststands.NotImplemented();
                    //    continue;
                    //}

                    if(input.AvoidDouble)
                    {
                        string name;
                        try
                        {
                            name = ProviderManager.Get(input.Provider).NameFromUrl(input.Image);
                        }
                        catch
                        {
                            name = input.Image;
                        }
                        if (DownloadRepository.Contains(input.Image, input.Metadata, input.DownloadType) || (ImageRepository.Value.Containes(name) && string.IsNullOrEmpty(input.Metadata)))
                            continue;
                    }

                    var queue = input.DownloadStade == DownloadStade.Paused ? DownloadStade.Paused : DownloadStade.Queued;
                    items.Add(DownloadRepository.Add(input.Image, input.DownloadType, input.Schedule, input.Provider, input.AvoidDouble, input.RemoveImageOnFail, input.Metadata, queue));
                }

                if(items.Count > 0)
                    db.SaveChanges();
            }

            return items.Select(de => new DownloadItem(de)).ToArray();
        }
    }
}