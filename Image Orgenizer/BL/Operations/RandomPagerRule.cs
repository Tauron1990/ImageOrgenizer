using System;
using System.Linq;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.RandomPager)]
    public class RandomPagerRule : IOBusinessRuleBase<PagerInput, PagerOutput>
    {
        public override PagerOutput ActionImpl(PagerInput input)
        {
            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IImageRepository>();

                IQueryable<ImageEntity> query = repo.QueryAsNoTracking()
                    .Include(e => e.ImageTags)
                    .ThenInclude(t => t.TagEntity)
                    .ThenInclude(te => te.Type);

                query = input.Reverse ? query.OrderByDescending(e => e.RandomCount) : query.OrderBy(e => e.RandomCount);

                if (input.Favorite)
                    query = query.Where(e => e.Favorite);

                if (input.TagFilter != null) query = input.TagFilter.Aggregate(query, FilterTag);

                var groups = query.Select(e => new RadomPagerHelper(e.RandomCount, e.Name)).GroupBy(helper => helper.RandomView).ToArray();

                var min = groups.Min(g => g.Key);

                var randomlist = groups.First(g => g.Key == min).OrderBy(e => Guid.NewGuid()).Take(input.Count);

                var readyList = randomlist.Select(helper => repo.QueryAsNoTracking()
                        .Include(e => e.ImageTags)
                        .ThenInclude(t => t.TagEntity)
                        .ThenInclude(te => te.Type)
                        .First(e => e.Name == helper.Name))
                    .Select(image => new ImageData(image))
                    .ToList();

                return new PagerOutput(0, readyList, 0);
            }
        }

        private static IQueryable<ImageEntity> FilterTag(IQueryable<ImageEntity> input, string tag) => input.Where(e => e.ImageTags.Select(it => it.TagEntity).Any(t => t.Id == tag));
    }
}