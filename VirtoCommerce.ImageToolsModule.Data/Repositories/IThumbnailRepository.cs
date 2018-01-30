namespace VirtoCommerce.ImageToolsModule.Data.Repositories
{
    using System.Linq;
    using System.Web.UI.WebControls.WebParts;

    using VirtoCommerce.ImageToolsModule.Data.Models;

    public interface IThumbnailRepository
    {
        /// <summary>
        /// Gets the thumbnail tasks.
        /// </summary>
        IQueryable<ThumbnailTaskEntity> ThumbnailTasks { get; }

        /// <summary>
        /// Gets the thumbnail options.
        /// </summary>
        IQueryable<ThumbnailOptionEntity> ThumbnailOptions { get; }

        ThumbnailTaskEntity[] GetThumbnailTasksByIds(string[] ids);

        ThumbnailOptionEntity[] GetThumbnailOptionsByIds(string[] ids);
        void DeletedThumbnailTasksByIds(string[] ids);
        void DeletedThumbnailOptionsByIds(string[] ids);
    }
}