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
    public class ThumbnailOptionSearchService : SearchService<ThumbnailOptionSearchCriteria, ThumbnailOptionSearchResult, ThumbnailOption, ThumbnailOptionEntity>, IThumbnailOptionSearchService
    {
        public ThumbnailOptionSearchService(
            Func<IThumbnailRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IThumbnailOptionService crudService,
            IOptions<CrudOptions> crudOptions)
            : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
        {
        }

        protected override IQueryable<ThumbnailOptionEntity> BuildQuery(IRepository repository, ThumbnailOptionSearchCriteria criteria)
        {
            return ((IThumbnailRepository)repository).ThumbnailOptions;
        }

        protected override IList<SortInfo> BuildSortExpression(ThumbnailOptionSearchCriteria criteria)
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
