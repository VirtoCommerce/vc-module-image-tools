namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    using VirtoCommerce.ImageToolsModule.Core.Models;
    using VirtoCommerce.ImageToolsModule.Core.Services;
    using VirtoCommerce.ImageToolsModule.Data.Repositories;

    public class ThumbnailTaskSearchService : IThumbnailTaskSearchService
    {
        private readonly IThumbnailRepository _repository;

        public ThumbnailTaskSearchService(IThumbnailRepository repository)
        {
            this._repository = repository;
        }

        public GenericSearchResponse<ThumbnailTask> SerchTasks(ThumbnailOptionSearchCriteria criteria)
        {
            throw new System.NotImplementedException();
        }
    }
}