using System;
using System.Linq;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.DownloadFailed)]
    public class DownloadFailedRule : IBusinessRuleBase<DownloadItem>
    {
        public override void ActionImpl(DownloadItem input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();

                var downloadEntity = repo.Get(true).First(e => e.Id == input.Id);
                downloadEntity.FailedCount++;
                downloadEntity.Schedule = DateTime.Now + TimeSpan.FromHours(1);

                if (downloadEntity.FailedCount == 10)
                {
                    downloadEntity.DownloadStade = DownloadStade.Failed;
                    if (downloadEntity.RemoveImageOnFail)
                    {
                        var imageRepo = RepositoryFactory.GetRepository<IImageRepository>();
                        var img = imageRepo.Query()
                            .Include(e => e.ImageTags)
                            .ThenInclude(e => e.TagEntity)
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