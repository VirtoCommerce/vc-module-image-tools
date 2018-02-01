using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Data.Models
{
    public class ThumbnailTaskEntity : AuditableEntity
    {
        [Required]
        [StringLength(1024)]
        public string Name { get; set; }

        public DateTime? LastRun { get; set; }

        [Required]
        [StringLength(2048)]
        public string WorkPath { get; set; }

        public ObservableCollection<ThumbnailTaskOptionEntity> ThumbnailTaskOptions { get; set; }

        public ThumbnailTaskEntity FromModel(ThumbnailTask task, PrimaryKeyResolvingMap pkMap)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            
            pkMap.AddPair(task, this);
            
            Name = task.Name;
            LastRun = task.LastRun;
            WorkPath = task.WorkPath;
            CreatedBy = task.CreatedBy;
            CreatedDate = task.CreatedDate;
            ModifiedBy = task.ModifiedBy;
            ModifiedDate = task.ModifiedDate;

            var optionEntitys = task.ThumbnailOptions.Select(o =>
            {
                var optionEntity = new ThumbnailOptionEntity();
                return optionEntity.FromModel(o, pkMap);
            });

            foreach (var taskOptionEntity in ThumbnailTaskOptions)
            {
                taskOptionEntity.OptionEntity
            }
                        
            return this;
        }

        public ThumbnailTask ToModel(ThumbnailTask task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            task.Name = Name;
            task.LastRun = LastRun;
            task.WorkPath = WorkPath;
            
            task.CreatedBy = CreatedBy;
            task.CreatedDate = CreatedDate;
            task.ModifiedBy = ModifiedBy;
            task.ModifiedDate = ModifiedDate;

            task.ThumbnailOptions = ThumbnailTaskOptions.Select(o => o.OptionEntity.ToModel(new ThumbnailOption()))
                .ToArray();
            
            return task;
        }

        public void Patch(ThumbnailTaskEntity target)
        {
            target.Id = Id;
            target.LastRun = LastRun;
            target.Name = Name;
            target.WorkPath = WorkPath;
        }
    }
}
