using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.ImageToolsModule.Core;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class ThumbnailGenerationProcessorTests : BlobChangesProviderTestBase
    {
        private readonly Mock<IThumbnailGenerator> _generator;
        private readonly Mock<IThumbnailHandlerFactory> _handlerFactory;
        private readonly Mock<ISettingsManager> _settingsManager;

        public ThumbnailGenerationProcessorTests()
        {
            _generator = new Mock<IThumbnailGenerator>();
            _handlerFactory = new Mock<IThumbnailHandlerFactory>();
            // Setup handler factory to return null (fall back to generator)
            _handlerFactory.Setup(x => x.GetHandlerAsync(It.IsAny<string>()))
                .ReturnsAsync((IFormatThumbnailHandler)null);

            var settingDescriptor = ModuleConstants.Settings.General.ProcessBatchSize;
            _settingsManager = new Mock<ISettingsManager>();
            _settingsManager.Setup(x => x.GetObjectSettingAsync(It.Is<string>(x => x.Equals(settingDescriptor.Name)), null, null))
                .Returns(Task.FromResult(new ObjectSettingEntry() { Value = settingDescriptor.DefaultValue }));
        }

        [Fact]
        public async Task ProcessTasksAsync_BlobChangesProviderCache_WorkingDuringOneRun()
        {
            // Arrange
            var blobContents = new List<BlobEntry>()
            {
                new BlobInfo()
                {
                    Name = "Blob1.png",
                    Url = "testPath/Blob1.png",
                },
            };
            var imageChangesProvider = GetBlobImagesChangesProvider(blobContents);
            var thumbnailGenerationProcessor = new ThumbnailGenerationProcessor(_generator.Object, _handlerFactory.Object, _settingsManager.Object, imageChangesProvider, Mock.Of<ILogger<ThumbnailGenerationProcessor>>());

            var thumbnailOption = new ThumbnailOption() { FileSuffix = OptionSuffix };
            var workPath = "testPath";
            var task1 = new ThumbnailTask()
            {
                Id = Guid.NewGuid().ToString(),
                LastRun = null,
                WorkPath = workPath,
                ThumbnailOptions = new List<ThumbnailOption>() { thumbnailOption },
            };
            var cancellationToken = new CancellationTokenWrapper(new CancellationToken());

            // Act

            await thumbnailGenerationProcessor.ProcessTasksAsync(new[] { task1 }, false, x => { }, cancellationToken);

            //Assert
            StorageProviderMock.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        }

        [Fact]
        public async Task ProcessTasksAsync_BlobChangesProviderCache_ExpiredAfterExecution()
        {
            // Arrange
            var blobContents = new List<BlobEntry>()
            {
                new BlobInfo()
                {
                    Name = "Blob1.png",
                    Url = "testPath/Blob1.png",
                },
            };
            var imageChangesProvider = GetBlobImagesChangesProvider(blobContents);
            var thumbnailGenerationProcessor = new ThumbnailGenerationProcessor(_generator.Object, _handlerFactory.Object, _settingsManager.Object, imageChangesProvider, Mock.Of<ILogger<ThumbnailGenerationProcessor>>());

            var thumbnailOption = new ThumbnailOption() { FileSuffix = OptionSuffix };
            var workPath = "testPath";
            var task1 = new ThumbnailTask()
            {
                Id = Guid.NewGuid().ToString(),
                LastRun = null,
                WorkPath = workPath,
                ThumbnailOptions = new List<ThumbnailOption>() { thumbnailOption },
            };
            var cancellationToken = new CancellationTokenWrapper(new CancellationToken());

            // Act

            await thumbnailGenerationProcessor.ProcessTasksAsync(new[] { task1 }, false, x => { }, cancellationToken);
            await thumbnailGenerationProcessor.ProcessTasksAsync(new[] { task1 }, false, x => { }, cancellationToken);

            //Assert
            StorageProviderMock.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }
    }
}
