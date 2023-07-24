using System.Collections.Generic;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.ImageToolsModule.Core.Events
{
    public class ThumbnailTaskChangedEvent : GenericChangedEntryEvent<ThumbnailTask>
    {
        public ThumbnailTaskChangedEvent(IEnumerable<GenericChangedEntry<ThumbnailTask>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
