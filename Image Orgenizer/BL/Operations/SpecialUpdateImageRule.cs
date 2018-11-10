using System;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.SpecialUpdateImage)]
    public class SpecialUpdateImageRule : IBusinessRuleBase<ImageData>
    {
        public override void ActionImpl(ImageData input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();
                repo.Add(input.Name, FileContainerManager.Contains(input.Name) ? DownloadType.UpdateTags : DownloadType.DownloadImage,
                    DateTime.Now + TimeSpan.FromMinutes(5), input.ProviderName, false, false);
            }
        }
    }
}