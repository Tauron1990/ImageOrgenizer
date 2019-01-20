using System.Collections.Generic;
using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Core;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.SearchLocation)]
    public class SearchLocationRule : IOBusinessRuleBase<string, ProfileData>
    {
        public override ProfileData ActionImpl(string input)
        {
            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IImageRepository>();
                List<string> names = repo.QueryAsNoTracking(false).OrderBy(ie => ie.SortOrder).Select(ie => ie.Name).ToList();
                int index = names.FindIndex(pr => pr.Contains(input));

                return new ProfileData(index + CommonApplication.Current.Container.Resolve<ISettingsManager>().Settings?.PageCount ?? 20, 
                    0, string.Empty, index, AppConststands.OrderedPager, false);

                //if (index == -2) return null;
                //if (index == -1)
                //    index = 0;

                //int pageCount = CommonApplication.Current.Container.Resolve<ISettingsManager>().Settings?.PageCount ?? 20;
                //int page = index / pageCount;
                //int pageIndex = index - pageCount * page;

                //return new ProfileData(page + 1, pageIndex, string.Empty, page, AppConststands.OrderedPager, false);
            }
        }
    }
}