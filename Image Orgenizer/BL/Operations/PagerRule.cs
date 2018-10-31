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

                if (input.Reverse)
                {
                    int rest = input.Next - input.Count;
                    if (rest < 0)
                        rest = input.Count - input.Next;
                    else
                        rest = 0;

                    int actualIndex = input.Next - (input.Count - rest);
                    bool small;
                    List<ImageEntity> ent;
                    if (actualIndex > 0)
                        ent = new List<ImageEntity>(QueryImages(new PagerInput(actualIndex, input.Count - rest, true, input.Favorite, input.TagFilter), repo, out small));
                    else
                    {
                        small = false;
                        ent = new List<ImageEntity>();
                    }

                    if (small)
                        return new PagerOutput(ent.Count, ent.Select(ie => new ImageData(ie)).ToList(), 0);

                    int start;
                    if (rest > 0)
                    {
                        int last = CreateQuery(input, repo).Count();
                        ent.AddRange(QueryImages(new PagerInput(last - rest, rest, true, input.Favorite, input.TagFilter), repo, out _));
                        start = last - rest;
                    }
                    else
                        start = actualIndex;

                    return new PagerOutput(input.Next, ent.Select(ie => new ImageData(ie)).ToList(), start);
                }
                else
                {
                    var targetCount = input.Count;


                    var imageEntities = new List<ImageEntity>();
                    bool small;
                    if (!input.Reverse || input.Next != 0)
                        imageEntities.AddRange(QueryImages(input, repo, out small));
                    else
                        small = false;

                    int next;
                    if (imageEntities.Count == targetCount)
                        next = input.Next + targetCount;
                    else if (small)
                        next = 0;
                    else
                    {
                        var newInput = new PagerInput(0, targetCount - imageEntities.Count, input.Reverse, input.Favorite, input.TagFilter);
                        var tempList = QueryImages(newInput, repo, out small).ToList();
                        next = tempList.Count - 1;

                        imageEntities.AddRange(tempList);
                    }

                    return new PagerOutput(next, imageEntities.Select(ie => new ImageData(ie)).ToList(), input.Next);

                }
            }
        }

        private IEnumerable<ImageEntity> QueryImages(PagerInput input, IImageRepository repo, out bool small)
        {
            var count = repo.QueryAsNoTracking().Take(input.Count).Select(ie => ie.Id).ToArray();
            if (count.Length < input.Count)
            {
                small = true;
                return new List<ImageEntity>(repo.Query());
            }

            small = false;

            var query = CreateQuery(input, repo);

            return query.Skip(input.Next).Take(input.Count);
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