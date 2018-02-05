using System.Threading;
using System.Threading.Tasks;

using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    public interface IThumbnailGenerator
    {
        Task<ThumbnailGenerationResult> GenerateThumbnailsAsync(string sourse, string destination, ThumbnailOption option, CancellationToken token);
    }
}