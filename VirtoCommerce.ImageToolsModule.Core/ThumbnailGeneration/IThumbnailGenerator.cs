using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;

namespace VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration
{
    public interface IThumbnailGenerator
    {
        Task<ThumbnailGenerationResult> GenerateThumbnailsAsync(string sourse, string destination, ThumbnailOption option, CancellationToken token);
    }
}