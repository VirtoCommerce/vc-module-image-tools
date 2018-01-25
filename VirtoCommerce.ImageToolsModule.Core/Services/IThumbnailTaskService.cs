namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    using VirtoCommerce.ImageToolsModule.Core.Models;

    public interface IThumbnailTaskService
    {
        void SaveChanges(ThumbnailTask[] options);

        ThumbnailTask[] GetByIds(string[] ids);

        void Delete(string[] ids);
    }
}