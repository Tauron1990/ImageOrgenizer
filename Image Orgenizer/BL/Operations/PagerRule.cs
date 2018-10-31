using System.Collections.Generic;
using System.Linq;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.Pager)]
    public sealed class PagerRule : IOBusinessRuleBase<PagerInput, PagerOutput>
    {
        public override PagerOutput ActionImpl(PagerInput input)
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

        //private IEnumerable<ImageEntity> QueryImages(PagerInput input, IImageRepository repo, out bool small)
        //{
        //    var count = repo.QueryAsNoTracking().Take(input.Count).Select(ie => ie.Id).ToArray();
        //    if (count.Length < input.Count)
        //    {
        //        small = true;
        //        return new List<ImageEntity>(repo.Query());
        //    }

        //    small = false;

        //    var query = CreateQuery(input, repo);

        //    return query.Skip(input.Next).Take(input.Count);
        //}

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
            IQueryable<ImageEntity> query = repo.QueryAsNoTracking()
                .Include(e => e.ImageTags)
                .ThenInclude(t => t.TagEntity);

            if (input.Favorite)
                query = query.Where(e => e.Favorite);

            if (input.TagFilter != null) query = input.TagFilter.Aggregate(query, FilterTag);

            return  query.OrderBy(e => e.SortOrder);
        }

        private static IQueryable<ImageEntity> FilterTag(IQueryable<ImageEntity> input, string tag) => input.Where(e => e.ImageTags.Select(it => it.TagEntity).Any(t => t.Id == tag));
    }
}