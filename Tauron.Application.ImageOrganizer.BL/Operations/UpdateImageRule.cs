﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tauron.Application.Common.BaseLayer;
using Tauron.Application.Common.BaseLayer.Core;
using Tauron.Application.Common.BaseLayer.Data;
using Tauron.Application.ImageOrganizer.BL.Operations.Helper;
using Tauron.Application.ImageOrganizer.Data.Entities;
using Tauron.Application.ImageOrganizer.Data.Repositories;

namespace Tauron.Application.ImageOrganizer.BL.Operations
{
    [ExportRule(RuleNames.UpdateImage)]
    public class UpdateImageRule : IOBusinessRuleBase<ImageData[], ImageData[]>
    {
        private class QueryCacheHelper<TEntity, TData, TKey, TEntityKey>
            where TEntity : GenericBaseEntity<TEntityKey>
        {
            private readonly Func<TKey, TEntity> _query;
            private readonly Func<TData, TEntity> _builder;
            private readonly Dictionary<TKey, TEntity> _cache = new Dictionary<TKey, TEntity>();
            private readonly Action<TEntity> _add;

            public QueryCacheHelper(Func<TKey, TEntity> query, Func<TData, TEntity> builder, Action<TEntity> add)
            {
                _query = query;
                _builder = builder;
                _add = add;
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
                _add(entity);
                _cache[key] = entity;

                return entity;
            }
        }

        public override ImageData[] ActionImpl(ImageData[] inputs)
        {
            if(inputs.Length == 0) return new ImageData[0];

            using (var db = RepositoryFactory.Enter())
            {
                var imageRepo = db.GetRepository<IImageRepository>();
                var tagRepo = db.GetRepository<ITagRepository>();
                var tagTypeRepo = db.GetRepository<ITagTypeRepository>();
               
                bool needSort = false;

                var tagCache = new QueryCacheHelper<TagEntity, TagData, string, int>(id => tagRepo.GetName(id, true), data => new TagEntity
                {
                    Description = data.Description,
                    Name =  data.Name
                }, tagRepo.Add);
                var tagTypeCache = new QueryCacheHelper<TagTypeEntity, TagTypeData, string, string>(id => tagTypeRepo.Get(id, true), data => new TagTypeEntity
                {
                    Id = data.Name,
                    Color = data.Color ?? "black"
                }, tagTypeRepo.Add);

                List<ImageData> result = new List<ImageData>();

                foreach(var input in inputs)
                {
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
                    {
                        image = imageRepo.Query(true)
                            .SingleOrDefault(e => e.Id == input.Id);
                    }

                    if(image == null) continue;

                    image.Author = image.Author;
                    image.Added = image.Added;
                    image.Favorite = image.Favorite;


                    foreach (var inputTag in input.Tags)
                    {
                        var imageTag = image.Tags?.FirstOrDefault(it => it.TagEntity.Name == inputTag.Name);
                        if (imageTag != null)
                        {
                            tagCache.Add(imageTag.TagEntity.Name, imageTag.TagEntity);
                            tagTypeCache.Add(imageTag.TagEntity.Type.Id, imageTag.TagEntity.Type);

                            if (!string.IsNullOrWhiteSpace(inputTag.Description))
                                imageTag.TagEntity.Description = inputTag.Description;
                            continue;
                        }


                        var tag = tagCache.GetEntity(inputTag.Name, inputTag);
                        
                        if (tag.Type == null)
                            tag.Type = tagTypeCache.GetEntity(inputTag.Type.Name, inputTag.Type);

                        if(image.Tags == null)
                            image.Tags = new ObservableCollection<ImageTag>();

                        var ite = new ImageTag
                        {
                            ImageEntity = image,
                            TagEntity = tag
                        };

                        image.Tags.Add(ite);
                        //imageTagRepo.Add(ite);
                    }

                    result.Add(input.New ? new ImageData(image, NaturalStringComparer.Comparer) : input);
                }

                db.SaveChanges();

                if (!needSort) return result.ToArray();

                imageRepo.SetOrder(ImageNaturalStringComparer.Comparer);
                db.SaveChanges();

                return result.ToArray();
            }
        }
    }
}