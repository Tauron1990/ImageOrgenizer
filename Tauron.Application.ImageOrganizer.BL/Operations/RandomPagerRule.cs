using System;
using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.BL.Operations.Helper;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.RandomPager)]
    public class RandomPagerRule : IOBusinessRuleBase<PagerInput, PagerOutput>
    {
        public override PagerOutput ActionImpl(PagerInput input)
        {
            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IImageRepository>();

                IQueryable<ImageEntity> query = repo.QueryAsNoTracking(true);

                query = query.OrderBy(e => e.RandomCount);

                if (input.Favorite)
                    query = query.Where(e => e.Favorite);

                if (input.TagFilter != null) query = input.TagFilter.Aggregate(query, FilterTag);

                var groups = query.Select(e => new RadomPagerHelper(e.RandomCount, e.Name)).GroupBy(helper => helper.RandomView).ToArray();

                var min = groups.Min(g => g.Key);

                var randomlist = groups.First(g => g.Key == min).OrderBy(e => Guid.NewGuid()).Take(input.Count);

                var readyList = randomlist.Select(helper => repo.QueryAsNoTracking(true)
                        .First(e => e.Name == helper.Name))
                    .Select(image => new ImageData(image, NaturalStringComparer.Comparer))
                    .ToList();

                return new PagerOutput(0, readyList, 0);
            }
        }

        private static IQueryable<ImageEntity> FilterTag(IQueryable<ImageEntity> input, string tag) => input.Where(e => e.Tags.Select(it => it.TagEntity).Any(t => t.Name == tag));
    }
}