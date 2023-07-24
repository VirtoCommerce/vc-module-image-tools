using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    public class ThumbnailTaskSearchService : SearchService<ThumbnailTaskSearchCriteria, ThumbnailTaskSearchResult, ThumbnailTask, ThumbnailTaskEntity>, IThumbnailTaskSearchService
    {
        public ThumbnailTaskSearchService(
            Func<IThumbnailRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IThumbnailTaskService crudService,
            IOptions<CrudOptions> crudOptions)
            : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
        {
        }

        protected override IQueryable<ThumbnailTaskEntity> BuildQuery(IRepository repository, ThumbnailTaskSearchCriteria criteria)
        {
            var query = ((IThumbnailRepository)repository).ThumbnailTasks;

            if (!criteria.Keyword.IsNullOrEmpty())
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword));
            }

            return query;
        }

        protected override IList<SortInfo> BuildSortExpression(ThumbnailTaskSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;

            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo {
                        SortColumn = nameof(ThumbnailOptionEntity.ModifiedDate),
                        SortDirection = SortDirection.Descending,
                    }
                };
            }

            return sortInfos;
        }
    }
}
