using System.Linq;
using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.IncreaseViewCount)]
    public sealed class IncreaseViewCountRule : IBusinessRuleBase<IncreaseViewCountInput>
    {
        public override void ActionImpl(IncreaseViewCountInput input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IImageRepository>();
                var imageEntity = repo.Query().First(ie => ie.Name == input.Name);

                if (input.IsRandom)
                    imageEntity.RandomCount++;
                imageEntity.ViewCount++;

                db.SaveChanges();
            }
        }
    }
}