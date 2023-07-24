using System.Collections.Generic;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.ImageToolsModule.Core.Events
{
    public class ThumbnailTaskChangeEvent : GenericChangedEntryEvent<ThumbnailTask>
    {
        public ThumbnailTaskChangeEvent(IEnumerable<GenericChangedEntry<ThumbnailTask>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
