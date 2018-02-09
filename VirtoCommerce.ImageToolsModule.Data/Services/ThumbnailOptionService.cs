using System;
using System.Linq;

using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    public class ThumbnailOptionService : ServiceBase, IThumbnailOptionService
    {
        private readonly Func<IThumbnailRepository> _thumbnailRepositoryFactory;

        public ThumbnailOptionService(Func<IThumbnailRepository> thumbnailRepositoryFactory)
        {
            this._thumbnailRepositoryFactory = thumbnailRepositoryFactory;
        }

        public void SaveOrUpdate(ThumbnailOption[] options)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = this._thumbnailRepositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var existOptionEntities = repository.GetThumbnailOptionsByIds(options.Select(t => t.Id).ToArray());
                foreach (var option in options)
                {
                    var sourceOptionsEntity = AbstractTypeFactory<ThumbnailOptionEntity>.TryCreateInstance();
                    if (sourceOptionsEntity != null)
                    {
                        sourceOptionsEntity = sourceOptionsEntity.FromModel(option, pkMap);
                        var targetOptionsEntity = existOptionEntities.FirstOrDefault(x => x.Id == option.Id);
                        if (targetOptionsEntity != null)
                        {
                            changeTracker.Attach(targetOptionsEntity);
                            sourceOptionsEntity.Patch(targetOptionsEntity);
                        }
                        else
                        {
                            repository.Add(sourceOptionsEntity);
                        }
                    }
                }

                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
            }
        }

        public ThumbnailOption[] GetByIds(string[] ids)
        {
            using (var repository = this._thumbnailRepositoryFactory())
            {
                return repository.GetThumbnailOptionsByIds(ids)
                    .Select(x => x.ToModel(AbstractTypeFactory<ThumbnailOption>.TryCreateInstance())).ToArray();
            }
        }

        public void RemoveByIds(string[] ids)
        {
            using (var repository = this._thumbnailRepositoryFactory())
            {
                repository.RemoveThumbnailTasksByIds(ids);
                CommitChanges(repository);
            }
        }
    }
}
