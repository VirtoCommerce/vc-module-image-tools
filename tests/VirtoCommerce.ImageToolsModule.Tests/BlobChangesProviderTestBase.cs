using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public abstract class BlobChangesProviderTestBase
    {
        public readonly string OptionSuffix = "64x64";

        public Mock<IBlobStorageProvider> StorageProviderMock { get; private set; }
        public Mock<IThumbnailOptionSearchService> ThumbnailOptionSearchServiceMock { get; private set; }
        public Mock<IImageService> ImageServiceMock { get; private set; }

        protected BlobChangesProviderTestBase()
        {
            StorageProviderMock = new Mock<IBlobStorageProvider>();
            ThumbnailOptionSearchServiceMock = new Mock<IThumbnailOptionSearchService>();
            ImageServiceMock = new Mock<IImageService>();
        }

        public IImagesChangesProvider GetBlobImagesChangesProvider(IEnumerable<BlobEntry> blobContents)
        {
            StorageProviderMock.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((folderUrl, keyword) =>
                {

                    var searchResult = blobContents.Where(x => x.Url.StartsWith(folderUrl)).ToList();

                    return Task.FromResult(new BlobEntrySearchResult()
                    {
                        TotalCount = searchResult.Count,
                        Results = searchResult,
                    });
                });

            StorageProviderMock.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns<string>(
                    x => Task.FromResult(x.EndsWith(OptionSuffix) ? null : new BlobInfo() { })
                );

            ThumbnailOptionSearchServiceMock.Setup(x => x.SearchAsync(It.IsAny<ThumbnailOptionSearchCriteria>(), It.IsAny<bool>()))
                .ReturnsAsync(new ThumbnailOptionSearchResult()
                {
                    TotalCount = 1,
                    Results = new List<ThumbnailOption>()
                    {
                        new ThumbnailOption() { FileSuffix = OptionSuffix },
                    }
                });

            ImageServiceMock.Setup(x => x.IsExtensionAllowed(It.IsAny<string>()))
                .ReturnsAsync(true);

            var result = new BlobImagesChangesProvider(StorageProviderMock.Object, ThumbnailOptionSearchServiceMock.Object, ImageServiceMock.Object, GetPlatformMemoryCache());

            return result;
        }

        private IPlatformMemoryCache GetPlatformMemoryCache()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            return new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);
        }
    }
}
