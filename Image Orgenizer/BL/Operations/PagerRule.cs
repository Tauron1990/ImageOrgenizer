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
            var targetCount = input.Count;
            List<ImageEntity> imageEntities;
            int next;

            using (RepositoryFactory.Enter())
            {
                var repo = RepositoryFactory.GetRepository<IImageRepository>();

                imageEntities = QueryImages(input, repo, out var small);

                if (imageEntities.Count == targetCount)
                    next = input.Next + targetCount;
                else if (small)
                    next = 0;
                else
                {
                    var newInput = new PagerInput(0, targetCount - imageEntities.Count, input.Reverse, input.Favorite, input.TagFilter);
                    var tempList = QueryImages(newInput, repo, out small);

                    var temp = tempList.Last();
                    next = tempList.Count;
                    tempList.Remove(temp);

                    imageEntities.AddRange(tempList);
                }
            }

            if (input.Reverse)
                imageEntities.Reverse();

            return new PagerOutput(next, imageEntities.Select(ie => new ImageData(ie)).ToList());
        }

        private List<ImageEntity> QueryImages(PagerInput input, IImageRepository repo, out bool small)
        {
            var count = repo.Query().Take(input.Count).Select(ie => ie.Id).ToArray();
            if (count.Length < 20)
            {
                small = true;
                return new List<ImageEntity>(repo.Query());
            }

            small = false;
            IQueryable<ImageEntity> query = repo.QueryAsNoTracking()
                .Include(e => e.ImageTags)
                .ThenInclude(t => t.TagEntity)
                .ThenInclude(te => te.Type);

            query = input.Reverse ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name);

            if (input.Favorite)
                query = query.Where(e => e.Favorite);

            if (input.TagFilter != null) query = input.TagFilter.Aggregate(query, FilterTag);

            return query.Skip(input.Next)
                .Take(input.Count)
                .ToList();
        }

        private static IQueryable<ImageEntity> FilterTag(IQueryable<ImageEntity> input, string tag) => input.Where(e => e.ImageTags.Select(it => it.TagEntity).Any(t => t.Id == tag));
    }
}