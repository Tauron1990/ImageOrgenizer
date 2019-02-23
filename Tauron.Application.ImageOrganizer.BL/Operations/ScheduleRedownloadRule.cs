using System;
using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.ScheduleRedownload)]
    public class ScheduleRedownloadRule : IOBusinessRuleBase<string, bool>
    {
        [InjectRepo]
        public IImageRepository ImageRepository { get; set; }

        [InjectRepo]
        public IDownloadRepository DownloadRepository { get; set; }

        public override bool ActionImpl(string input)
        {
            //AppConststands.NotImplemented();
            //return false;


            using (var db = Enter())
            {
                var img = ImageRepository.QueryAsNoTracking(false).FirstOrDefault(e => e.Name == input);

                if (img == null)
                    return false;

                DownloadRepository.Add(input, DownloadType.ReDownload, DateTime.Now + TimeSpan.FromMinutes(5), 
                    img.ProviderName, false, false, String.Empty);

                db.SaveChanges();
                return true;
            }

        }
    }
}