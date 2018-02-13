using System.Threading;
using System.Threading.Tasks;

using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    public interface IThumbnailGenerator
    {
        ThumbnailGenerationResult GenerateThumbnailsAsync(string sourse, string destination, ThumbnailOption option, ICancellationToken token);
    }
}