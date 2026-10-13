using Microsoft.Extensions.Logging.Abstractions;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.Services;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;

namespace VirtoCommerce.ImageToolsModule.Tests.Imaging;

/// <summary>
/// Composes the real raster thumbnail pipeline (no mocks) over the given blob storage.
/// This is the only place in the tests that knows which image services implement the pipeline.
/// </summary>
internal static class ThumbnailTestHost
{
    public static IThumbnailGenerator CreateGenerator(IBlobStorageProvider blobStorage)
    {
        var imageService = new ImageService(blobStorage, NullLogger<ImageService>.Instance);

        return new ThumbnailGenerator(imageService, new ImageResizer(), NullLogger<ThumbnailGenerator>.Instance);
    }

    public static IFormatThumbnailHandler CreateRasterHandler(IBlobStorageProvider blobStorage)
    {
        return new RasterThumbnailHandler(CreateGenerator(blobStorage));
    }
}
