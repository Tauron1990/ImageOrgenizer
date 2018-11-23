﻿using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.ScheduleRedownload)]
    public class ScheduleRedownloadRule : IOBusinessRuleBase<string, bool>
    {
        public override bool ActionImpl(string input)
        {
            AppConststands.NotImplemented();
            return false;

/*
            using (var db = RepositoryFactory.Enter())
            {
                var imgRepo = db.GetRepository<IImageRepository>();
                var dowRepo = db.GetRepository<IDownloadRepository>();

                var img = imgRepo.QueryAsNoTracking(false).FirstOrDefault(e => e.Name == input);

                if (img == null)
                    return false;

                dowRepo.Add(input, DownloadType.ReDownload, DateTime.Now + TimeSpan.FromMinutes(5), img.ProviderName, false, false);

                db.SaveChanges();
                return true;
            }
*/
        }
    }
}