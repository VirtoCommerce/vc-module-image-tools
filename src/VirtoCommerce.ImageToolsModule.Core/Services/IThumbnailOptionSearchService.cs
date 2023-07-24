using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    public interface IThumbnailOptionSearchService : ISearchService<ThumbnailOptionSearchCriteria, ThumbnailOptionSearchResult, ThumbnailOption>
    {
    }
}
