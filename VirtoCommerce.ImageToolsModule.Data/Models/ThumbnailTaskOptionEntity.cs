using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Data.Models
{
    public class ThumbnailTaskOptionEntity : AuditableEntity
    {
        public string TaskId { get; set; }

        public ThumbnailTaskEntity TaskEntity { get; set; }

        public string OptionId { get; set; }

        public ThumbnailOptionEntity OptionEntity { get; set; }
    }
}