using System;
using Omu.ValueInjecter;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;

namespace VirtoCommerce.ImageToolsModule.Data.Models
{
    public class ThumbnailTaskOptionEntity : Entity
    {
        public string ThumbnailTaskId { get; set; }

        public ThumbnailTaskEntity ThumbnailTask { get; set; }

        public string ThumbnailOptionId { get; set; }

        public ThumbnailOptionEntity ThumbnailOption { get; set; }

        public virtual void Patch(ThumbnailTaskOptionEntity target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var patchInjection = new PatchInjection<ThumbnailTaskOptionEntity>(x => x.ThumbnailOptionId, x => x.ThumbnailTaskId);
            target.InjectFrom(patchInjection, this);

        }
    }

}