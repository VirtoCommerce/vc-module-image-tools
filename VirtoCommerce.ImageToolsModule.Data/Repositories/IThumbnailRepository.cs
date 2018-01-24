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

        IQueryable<ThumbnailOptionEntity> ThumbnailOptions { get; }

        ThumbnailTaskEntity[] GetThumbnailTasksByIds(string[] ids);

        ThumbnailOptionEntity[] GetThumbnailOptionsByIds(string[] ids);
    }
}