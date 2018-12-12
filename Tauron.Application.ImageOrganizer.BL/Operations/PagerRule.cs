using System;
using System.Collections.Generic;
using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.Pager)]
    public sealed class PagerRule : IOBusinessRuleBase<PagerInput, PagerOutput>
    {
        public override PagerOutput ActionImpl(PagerInput input)
        {
            try
            {
                using (RepositoryFactory.Enter())
                {
                    var repo = RepositoryFactory.GetRepository<IImageRepository>();

                    var query = CreateQuery(input, repo);
                    var all = query.Count();
                    var pages = ((all - 1) / input.Count + 1) - 1;
                    int realPage = input.Next;
                    if (realPage == -1)
                        realPage = pages;

                    List<ImageEntity> ent = query.Skip(realPage * input.Count).Take(input.Count).ToList();
                    var page = EvaluateNext(pages, realPage);

                    return new PagerOutput(page.Next, ent.Select(ie => new ImageData(ie)).ToList(), page.Start);

                }
            }
            catch (Exception e)
            {
                if (e.IsCriticalApplicationException()) throw;

                return new PagerOutput(0, new List<ImageData>(), 0);
            }
        }

        private (int Next, int Start) EvaluateNext(int pages, int current)
        {
            int next;
            int start;
            if (pages == 0)
            {
                next = 0;
                start = 0;
            }
            else
            {
                start = current;
                next = current + 1;
                if (next > pages)
                    next = 0;
            }

            return (next, start);
        }

        private IQueryable<ImageEntity> CreateQuery(PagerInput input, IImageRepository repo)
        {
            IQueryable<ImageEntity> query = repo.QueryAsNoTracking(true);

            if (input.Favorite)
                query = query.Where(e => e.Favorite);

            if (input.TagFilter != null) query = input.TagFilter.Aggregate(query, FilterTag);

            return  query.OrderBy(e => e.SortOrder);
        }

        private static IQueryable<ImageEntity> FilterTag(IQueryable<ImageEntity> input, string tag) => input.Where(e => e.Tags.Select(it => it.TagEntity).Any(t => t.Name == tag));
    }
}