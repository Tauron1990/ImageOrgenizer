using System.Linq;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Data.Repositories;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{ 
    [ExportRule(RuleNames.GetDownloadCount)]
    public class GetDownloadCountRule : OBusinessRuleBase<int>
    {
        public override int ActionImpl()
        {
            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();

                return repo.Get(false).Select(di => di.DownloadStade == DownloadStade.Compled).Count();
            }
        }
    }
}