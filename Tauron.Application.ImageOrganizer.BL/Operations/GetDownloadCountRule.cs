using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{ 
    [ExportRule(RuleNames.GetDownloadCount)]
    public class GetDownloadCountRule : OBusinessRuleBase<int>
    {
        public override int ActionImpl()
        {
            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IDownloadRepository>();
                
                return repo.Get(false).Count(di => di.DownloadStade == DownloadStade.Compled);
            }
        }
    }
}