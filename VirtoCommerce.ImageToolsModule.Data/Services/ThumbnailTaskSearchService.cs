using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Repositories;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    public class ThumbnailTaskSearchService : IThumbnailTaskSearchService
    {
        private IThumbnailRepository _repository;

        public ThumbnailTaskSearchService(IThumbnailRepository repository)
        {
            _repository = repository;
        }

        public GenericSearchResponse<ThumbnailTask> SerchTasks(ThumbnailOptionSearchCriteria criteria)
        {
            throw new System.NotImplementedException();
        }
        
        public GenericSearchResponse<ThumbnailTask> SerchTasks(string keyword)
        {
            throw new System.NotImplementedException();
        }
    }
}