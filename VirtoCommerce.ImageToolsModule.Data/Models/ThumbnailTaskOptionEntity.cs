using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Data.Models
{
    public class ThumbnailTaskOptionEntity : AuditableEntity
    {
        public string ThumbnailTaskEntityId { get; set; }

        public ThumbnailTaskEntity ThumbnailTaskEntity { get; set; }

        public string ThumbnailOptionEntityId { get; set; }

        public ThumbnailOptionEntity ThumbnailOptionEntity { get; set; }
    }
}