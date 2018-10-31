using System;
using System.Collections.Generic;
using System.Linq;
using ImageOrganizer.BL.Operations.Helper;
using ImageOrganizer.Data;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.Common.BaseLayer.Data;

namespace ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.UpdateImage)]
    public class UpdateImageRule : IOBusinessRuleBase<ImageData, ImageData>
    {
        private class QueryCacheHelper<TEntity, TData, TKey>
            where TEntity : GenericBaseEntity<TKey>
        {
            private readonly Func<TKey, TEntity> _query;
            private readonly Func<TData, TEntity> _builder;
            private readonly DbContext _context;
            private readonly Dictionary<TKey, TEntity> _cache = new Dictionary<TKey, TEntity>();

            public QueryCacheHelper(Func<TKey, TEntity> query, Func<TData, TEntity> builder, DbContext context)
            {
                _query = query;
                _builder = builder;
                _context = context;
            }

            public void Add(TKey key, TEntity entity)
            {
                if(entity == null || _cache.ContainsKey(key)) return;
                _cache[key] = entity;
            }

            public TEntity GetEntity(TKey key, TData data)
            {
                if (_cache.TryGetValue(key, out var entity)) return entity;

                entity = _query(key);
                if (entity != null) return entity;

                entity = _builder(data);
                _context.Add(entity);
                _cache[key] = entity;

                return entity;
            }
        }

        public override ImageData ActionImpl(ImageData input)
        {
            using (var db = RepositoryFactory.Enter())
            {
                var imageRepo = db.GetRepository<IImageRepository>();
                var tagRepo = db.GetRepository<ITagRepository>();
                var tagTypeRepo = db.GetRepository<ITagTypeRepository>();
                var context = db.GetContext<DatabaseImpl>();
                bool needSort = false;

                var tagCache = new QueryCacheHelper<TagEntity, TagData, string>(id => tagRepo.GetName(id, true), data => new TagEntity
                {
                    Description = data.Description,
                    Id =  data.Name
                }, context);
                var tagTypeCache = new QueryCacheHelper<TagTypeEntity, TagTypeData, string>(id => tagTypeRepo.Get(id, true), data => new TagTypeEntity
                {
                    Id = data.Name,
                    Color = data.Color
                }, context);

                ImageEntity image;
                if (input.New)
                {
                    image = new ImageEntity
                    {
                        Added = input.Added,
                        Author = input.Author,
                        Name = input.Name,
                        ProviderName = input.ProviderName
                    };

                    imageRepo.Add(image);
                    needSort = true;
                }
                else
                    image = imageRepo.Query()
                        .Include(e => e.ImageTags)
                        .ThenInclude(e => e.TagEntity)
                        .ThenInclude(e => e.Type)
                        .SingleOrDefault(e => e.Id == input.Id);

                if(image == null) return input;

                image.Author = image.Author;
                image.Added = image.Added;
                image.Favorite = image.Favorite;

                List<ImageTag> tags = new List<ImageTag>();

                foreach (var inputTag in input.Tags)
                {
                    var imageTag = image.ImageTags.FirstOrDefault(it => it.TagEntity.Id == inputTag.Name);
                    if (imageTag != null)
                    {
                        tagCache.Add(imageTag.TagEntityId, imageTag.TagEntity);
                        tagTypeCache.Add(imageTag.TagEntity.Type.Id, imageTag.TagEntity.Type);
                        tags.Add(imageTag);
                        continue;
                    }


                    var tag = tagCache.GetEntity(inputTag.Name, inputTag);

                    if (tag.Type == null)
                        tag.Type = tagTypeCache.GetEntity(inputTag.Type.Name, inputTag.Type);

                    tags.Add(new ImageTag
                    {
                        ImageEntity = image,
                        TagEntity =  tag
                    });
                }

                image.ImageTags.Clear();
                tags.ForEach(it => image.ImageTags.Add(it));

                if (needSort)
                    imageRepo.Query().ToList().SetOrder();

                db.SaveChanges();
                return input.New ? new ImageData(image) : input;
            }
        }
    }
}