using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Data.Models
{


    public class ThumbnailTaskEntity// : AuditableEntity
    {
        public string Id { get; set; }
        
        [Required]
        [StringLength(1024)]
        public string Name { get; set; }

        public DateTime? LastRun { get; set; }

        [Required]
        [StringLength(2048)]
        public string WorkPath { get; set; }

        public ObservableCollection<ThumbnailTaskOptionEntity> ThumbnailTaskOptions { get; set; }

        public ThumbnailTaskEntity FromModel(ThumbnailTask task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            Name = task.Name;
            LastRun = task.LastRun;
            WorkPath = task.WorkPath;

            return this;
        }

        public ThumbnailTask ToModel(ThumbnailTask task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            task.Name = Name;
            task.LastRun = LastRun;
            task.WorkPath = WorkPath;

            return task;
        }
    }
}
