using System.Collections.Generic;
using System.Linq;
using ImageOrganizer.Data.Repositories;
using ImageOrganizer.Views.Models;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.SearchLocation)]
    public class SearchLocationRule : IOBusinessRuleBase<string, ProfileData>
    {
        public override ProfileData ActionImpl(string input)
        {
            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IImageRepository>();
                List<string> names = repo.QueryAsNoTracking().OrderBy(ie => ie.SortOrder).Select(ie => ie.Name).ToList();
                int index = names.FindIndex(pr => pr.Contains(input)) - 1;
                if (index == -2) return null;
                if (index == -1)
                    index = 0;

                int pageCount = Properties.Settings.Default.PageCount;
                int page = index / pageCount;
                int pageIndex = index - pageCount * page;

                return new ProfileData(page + 1, pageIndex, string.Empty, page, ImageViewerModel.OrderedPager, false);
            }
        }
    }
}