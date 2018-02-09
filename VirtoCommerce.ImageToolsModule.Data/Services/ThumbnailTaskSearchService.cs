using System;
using System.Linq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    public class ThumbnailTaskSearchService : ServiceBase, IThumbnailTaskSearchService
    {
        private readonly Func<IThumbnailRepository> _thumbnailRepositoryFactory;

        public ThumbnailTaskSearchService(Func<IThumbnailRepository> thumbnailThumbnailRepositoryFactoryFactory)
        {
            _thumbnailRepositoryFactory = thumbnailThumbnailRepositoryFactoryFactory;
        }

        public GenericSearchResponse<ThumbnailTask> Search(ThumbnailTaskSearchCriteria criteria)
        {
            using (var repository = _thumbnailRepositoryFactory())
            {
                var sortInfos = criteria.SortInfos;

                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[]
                    {
                        new SortInfo
                        {
                            SortColumn = ReflectionUtility.GetPropertyName<ThumbnailTask>(t => t.CreatedDate),
                            SortDirection = SortDirection.Descending
                        }
                    };
                }

                var query = repository.ThumbnailTasks.OrderBySortInfos(sortInfos);

                var retVal = new GenericSearchResponse<ThumbnailTask> { TotalCount = query.Count() };

                var ids = query.Skip(criteria.Skip).Take(criteria.Take).Select(x => x.Id).ToArray();
                retVal.Results = repository.GetThumbnailTasksByIds(ids)
                    .Select(t => t.ToModel(AbstractTypeFactory<ThumbnailTask>.TryCreateInstance())).ToArray();

                return retVal;
            }
        }

        public GenericSearchResponse<ThumbnailTask> Search(string keyword)
        {
            return this.Search(new ThumbnailTaskSearchCriteria { SearchPhrase = keyword });
        }
    }
}