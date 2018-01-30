using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.Services
{

    public interface IThumbnailTaskSearchService
    {
        GenericSearchResponse<ThumbnailTask> SerchTasks(ThumbnailOptionSearchCriteria criteria);
    }
    
    public class GenericSearchResponse<T>
    {
    }
}