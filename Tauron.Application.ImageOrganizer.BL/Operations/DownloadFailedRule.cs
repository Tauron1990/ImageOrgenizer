using System;
using System.Linq;
using NLog;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.DownloadFailed)]
    public class DownloadFailedRule : IBusinessRuleBase<DownloadItem>
    {
        [InjectRepo]
        public IDownloadRepository DownloadRepository { get; set; }

        [InjectRepo]
        public Lazy<IImageRepository> ImageRepository { get; set; }

        public override void ActionImpl(DownloadItem input)
        {
            using (var db = Enter())
            {
                var downloadEntity = DownloadRepository.Get(true).First(e => e.Id == input.Id);
                downloadEntity.FailedCount++;
                downloadEntity.Schedule = DateTime.Now + TimeSpan.FromHours(1);
                downloadEntity.FailedReason = input.FailedReason;

                Logger.Trace($"Donwload Failed:{input.Image}--{input.Metadata}--{input.DownloadType}  Count:{downloadEntity.FailedCount}   Reason:{input.FailedReason}");

                if (downloadEntity.FailedCount == 10)
                {
                    Logger.Warn($"Donwload Failed:{input.Image}--{input.Metadata}--{input.DownloadType}  Count:{downloadEntity.FailedCount}   " +
                                 $"Reason:{input.FailedReason}  Remove:{downloadEntity.RemoveImageOnFail}");
                    downloadEntity.DownloadStade = DownloadStade.Failed;
                    if (downloadEntity.RemoveImageOnFail)
                    {
                        var imageRepo = ImageRepository.Value;
                        var img = imageRepo.Query(true)
                            .FirstOrDefault(ent => ent.Name == input.Image);

                        if(img != null)
                            imageRepo.Remove(img);
                    }
                }

                db.SaveChanges();
            }
        }
    }
}