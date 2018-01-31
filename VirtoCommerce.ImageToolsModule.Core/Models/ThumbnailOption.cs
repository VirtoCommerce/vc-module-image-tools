namespace VirtoCommerce.ImageToolsModule.Core.Models
{
    public class ThumbnailOption
    {
        public string Id { get; set; }
        
        public string Name { get; set; }

        public string FileSuffix { get; set; }

        public ResizeMethod ResizeMethod { get; set; }

        public decimal Width { get; set; }

        public decimal Height { get; set; }
    }
}