namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    using VirtoCommerce.ImageToolsModule.Core.Models;

    public interface IThumbnailOptionService
    {
        void SaveChanges(ThumbnailOption[] options);

        ThumbnailOption[] GetByIds(string[] ids);

        void Delete(string[] ids);
    }
}