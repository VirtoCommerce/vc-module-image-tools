using System;
using Omu.ValueInjecter;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;

namespace VirtoCommerce.ImageToolsModule.Data.Models
{
    public class ThumbnailTaskOptionEntity : Entity
    {
        public string ThumbnailTaskEntityId { get; set; }

        public ThumbnailTaskEntity ThumbnailTaskEntity { get; set; }

        public string ThumbnailOptionEntityId { get; set; }

        public ThumbnailOptionEntity ThumbnailOptionEntity { get; set; }

        public virtual void Patch(ThumbnailTaskOptionEntity target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var patchInjection = new PatchInjection<ThumbnailTaskOptionEntity>(x => x.ThumbnailOptionEntityId, x => x.ThumbnailTaskEntityId);
            target.InjectFrom(patchInjection, this);

        }

        public virtual ThumbnailTaskOptionEntity ToModel(ThumbnailOption option)
        {
            var result = new ThumbnailTaskOptionEntity();
            result.ThumbnailOptionEntityId = option.Id;
            return result;
        }
    }

}