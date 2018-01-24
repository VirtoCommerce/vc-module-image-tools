namespace VirtoCommerce.ImageToolsModule.Core.Models
{
    using System;
    using System.Collections.Generic;

    public class ThumbnailTask : AuditableEntity
    {
        public string Name { get; set; }

        public DateTime? LastRun { get; set; }

        public string WorkPath { get; set; }

        public IList<ThumbnailOption> ThumbnailOptions { get; set; }
    }
}