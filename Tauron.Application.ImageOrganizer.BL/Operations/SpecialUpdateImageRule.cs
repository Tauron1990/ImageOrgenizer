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
        public override void ActionImpl(ImageData input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();
                repo.Add(input.Name, DownloadType.DownloadTags, //FileContainerManager.Contains(input.Name) ? DownloadType.UpdateTags : DownloadType.DownloadImage,
                    DateTime.Now + TimeSpan.FromMinutes(5), input.ProviderName, false, false, null);

                db.SaveChanges();
            }
        }
    }
}