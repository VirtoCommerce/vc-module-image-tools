using System;

namespace VirtoCommerce.ImageToolsModule.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using VirtoCommerce.ImageToolsModule.Core.Models;
    using VirtoCommerce.Platform.Core.Common;

    class ThumbnailOptionEntity : AuditableEntity
    {
        [Required]
        [StringLength(1024)]
        public string Name { get; set; }

        [Required]
        [StringLength(128)]
        public string FileSuffix { get; set; }

        [Required]
        [StringLength(64)]
        public string ResizeMethod { get; set; }

        [Required]
        [StringLength(128)]
        public string ThumbnailTaskId { get; set; }

        public ThumbnailTaskOptionEntity ThumbnailTaskOptionEntity { get; set; }

        public ThumbnailOptionEntity FromModel(ThumbnailOption option)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));

            Name = option.Name;
            FileSuffix = option.FileSuffix;
            ResizeMethod = option.ResizeMethod.ToString();

            return this;
        }

        public ThumbnailOption ToModel(ThumbnailOption option)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));

            option.Name = Name;
            option.FileSuffix = FileSuffix;
            option.ResizeMethod = (ResizeMethod)Enum.Parse(typeof(ResizeMethod), ResizeMethod);

            return option;
        }
    }
}
