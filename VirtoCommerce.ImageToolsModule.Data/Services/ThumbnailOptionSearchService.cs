using System;
using System.Linq;

using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    public class ThumbnailOptionSearchService : ServiceBase, IThumbnailOptionSearchService
    {
        private readonly Func<IThumbnailRepository> _thumbnailRepositoryFactory;

        public ThumbnailOptionSearchService(Func<IThumbnailRepository> thumbnailRepositoryFactory)
        {
            this._thumbnailRepositoryFactory = thumbnailRepositoryFactory;
        }

        public GenericSearchResponse<ThumbnailOption> Search(ThumbnailOptionSearchCriteria criteria)
        {
            using (var repository = this._thumbnailRepositoryFactory())
            {
                var sortInfos = criteria.SortInfos;

                if (sortInfos.IsNullOrEmpty())
                    sortInfos = new[]
                                    {
                                        new SortInfo
                                            {
                                                SortColumn =
                                                    ReflectionUtility.GetPropertyName<ThumbnailTask>(
                                                        t => t.CreatedDate),
                                                SortDirection = SortDirection.Descending
                                            }
                                    };

                var query = repository.ThumbnailTasks.OrderBySortInfos(sortInfos);

                var retVal = new GenericSearchResponse<ThumbnailOption> { TotalCount = query.Count() };

                var ids = query.Skip(criteria.Skip).Take(criteria.Take).Select(x => x.Id).ToArray();
                retVal.Results = repository.GetThumbnailOptionsByIds(ids)
                    .Select(t => t.ToModel(AbstractTypeFactory<ThumbnailOption>.TryCreateInstance())).ToArray();

                return retVal;
            }
        }
    }
}