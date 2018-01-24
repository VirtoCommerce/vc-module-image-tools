namespace VirtoCommerce.ImageToolsModule.Data.Models
{
    internal class ThumbnailTaskOptionEntity
    {
        public string Id { get; set; }

        public string TaskId { get; set; }

        public ThumbnailTaskEntity TaskEntity { get; set; }

        public string OptionId { get; set; }

        public ThumbnailTaskOptionEntity OptionEntity { get; set; }
    }
}