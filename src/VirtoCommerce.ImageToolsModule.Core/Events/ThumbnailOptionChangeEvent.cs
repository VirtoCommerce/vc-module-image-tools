using System.Collections.Generic;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.ImageToolsModule.Core.Events
{
    public class ThumbnailOptionChangeEvent : GenericChangedEntryEvent<ThumbnailOption>
    {
        public ThumbnailOptionChangeEvent(IEnumerable<GenericChangedEntry<ThumbnailOption>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
