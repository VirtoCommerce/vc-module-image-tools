using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Events;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    public class ThumbnailTaskService : CrudService<ThumbnailTask, ThumbnailTaskEntity, ThumbnailTaskChangeEvent, ThumbnailTaskChangedEvent>, IThumbnailTaskService
    {
        public ThumbnailTaskService(
            Func<IThumbnailRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
        }

        protected override Task<IList<ThumbnailTaskEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return ((IThumbnailRepository)repository).GetThumbnailTasksByIdsAsync(ids);
        }
    }
}
