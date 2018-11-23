using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.Defrag)]
    public class DefragRule : IBusinessRuleBase<DefragInput>
    {
        public override void ActionImpl(DefragInput input)
        {
            string[] expected;
            using (RepositoryFactory.Enter())
                expected = RepositoryFactory.GetRepository<IImageRepository>().QueryAsNoTracking(false).Select(e => e.Name).ToArray();

            using (var trans = FileContainerManager.GetContainerTransaction())
            {
                try
                {
                    FileContainerManager.Defrag(expected, input.OnErrorFound, input.OnMessage, trans);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                }
            }
        }
    }
}