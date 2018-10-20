﻿using System;
using System.Linq;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.ScheduleRedownload)]
    public class ScheduleRedownloadRule : IBusinessRuleBase<string>
    {
        public override void ActionImpl(string input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var imgRepo = db.GetRepository<IImageRepository>();
                var dowRepo = db.GetRepository<IDownloadRepository>();

                var img = imgRepo.QueryAsNoTracking().FirstOrDefault(e => e.Name == input);

                if (img == null)
                    return;

                dowRepo.Add(input, DownloadType.ReDownload, DateTime.Now + TimeSpan.FromMinutes(5), img.ProviderName, false);

                db.SaveChanges();
            }
        }
    }
}