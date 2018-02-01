using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    public interface IThumbnailOptionService
    {
        void SaveThumbnailOptions(ThumbnailOption[] options);

        ThumbnailOption[] GetByIds(string[] ids);

        void RemoveByIds(string[] ids);
    }
}