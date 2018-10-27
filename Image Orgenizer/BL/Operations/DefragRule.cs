using System;
using System.Linq;
using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.Defrag)]
    public class DefragRule : IBusinessRuleBase<DefragInput>
    {
        public override void ActionImpl(DefragInput input)
        {
            string[] expected;
            using (RepositoryFactory.Enter())
                expected = RepositoryFactory.GetRepository<IImageRepository>().QueryAsNoTracking().Select(e => e.Name).ToArray();

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