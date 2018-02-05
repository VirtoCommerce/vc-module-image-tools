using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    public interface IThumbnailTaskService
    {
        void SaveOrUpdate(ThumbnailTask[] options);
        ThumbnailTask[] GetByIds(string[] ids);
        void RemoveByIds(string[] ids);
    }
}