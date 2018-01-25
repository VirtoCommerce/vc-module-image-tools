namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    using VirtoCommerce.ImageToolsModule.Core.Models;

    public interface IThumbnailTaskSearchService
    {
        GenericSearchResponse<ThumbnailTask> SerchTasks(ThumbnailOptionSearchCriteria criteria);
    }
}