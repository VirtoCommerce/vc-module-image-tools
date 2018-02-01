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
    public class ThumbnailTaskService : ServiceBase, IThumbnailTaskService
    {
        private readonly Func<IThumbnailRepository> _thumbnailRepositoryFactory;

        public ThumbnailTaskService(Func<IThumbnailRepository> thumbnailRepositoryFactory)
        {
            _thumbnailRepositoryFactory = thumbnailRepositoryFactory;
        }

        public void SaveThumbnailTasks(ThumbnailTask[] tasks)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = _thumbnailRepositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var existPlanEntities = repository.GetThumbnailTasksByIds(tasks.Select(t => t.Id).ToArray());
                foreach (var task in tasks)
                {
                    var sourceTaskEntity = AbstractTypeFactory<ThumbnailTaskEntity>.TryCreateInstance();
                    if (sourceTaskEntity != null)
                    {
                        sourceTaskEntity = sourceTaskEntity.FromModel(task, pkMap);
                        var targetTaskEntity = existPlanEntities.FirstOrDefault(x => x.Id == task.Id);
                        if (targetTaskEntity != null)
                        {
                            changeTracker.Attach(targetTaskEntity);
                            sourceTaskEntity.Patch(targetTaskEntity);
                        }
                        else
                        {
                            repository.Add(sourceTaskEntity);
                        }
                    }
                }
                
                pkMap.ResolvePrimaryKeys();
                CommitChanges(repository);
            }
        }

        public void RemoveByIds(string[] ids)
        {
            using (var repository = _thumbnailRepositoryFactory())
            {
                repository.RemoveThumbnailTasksByIds(ids);
                CommitChanges(repository);
            }
        }

        public ThumbnailTask[] GetByIds(string[] ids)
        {
            using (var repository = _thumbnailRepositoryFactory())
            {
                return repository.GetThumbnailTasksByIds(ids)
                    .Select(x => x.ToModel(AbstractTypeFactory<ThumbnailTask>.TryCreateInstance())).ToArray();
            }
        }
    }
}