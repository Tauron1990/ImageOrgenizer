using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.HasTag)]
    public class HasTagRule : IOBusinessRuleBase<string, bool>
    {
        public override bool ActionImpl(string input)
        {
            using (var db = RepositoryFactory.Enter())
                return db.GetRepository<ITagRepository>().Contains(input);
        }
    }
}