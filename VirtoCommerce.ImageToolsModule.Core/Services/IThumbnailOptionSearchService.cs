using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    public interface IThumbnailOptionSearchService
    {
        GenericSearchResponse<ThumbnailOption> Search(ThumbnailOptionSearchCriteria criteria);
    }
}
