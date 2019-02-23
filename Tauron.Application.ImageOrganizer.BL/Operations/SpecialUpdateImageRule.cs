using System;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.SpecialUpdateImage)]
    public class SpecialUpdateImageRule : IBusinessRuleBase<ImageData>
    {
        [InjectRepo]
        public IDownloadRepository DownloadRepository { get; set; }

        public override void ActionImpl(ImageData input)
        {
            using (var db = Enter())
            {
                DownloadRepository.Add(input.Name, FileContainerManager.Contains(input.Name) ? DownloadType.UpdateTags : DownloadType.DownloadImage,
                    DateTime.Now + TimeSpan.FromMinutes(5), input.ProviderName, false, false, null);

                db.SaveChanges();
            }
        }
    }
}