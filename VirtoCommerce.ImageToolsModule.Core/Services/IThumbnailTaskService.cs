using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    public interface IThumbnailTaskService
    {
        void SaveChanges(ThumbnailTask[] options);

        ThumbnailTask[] GetByIds(string[] ids);

        void Delete(string[] ids);
    }
}