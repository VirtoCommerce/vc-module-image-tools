using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    public interface IThumbnailOptionService
    {
        void SaveChanges(ThumbnailOption[] options);

        ThumbnailOption[] GetByIds(string[] ids);

        void Delete(string[] ids);
    }
}