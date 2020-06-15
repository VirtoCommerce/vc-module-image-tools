using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class BlobImagesChangesProviderTests : BlobChangesProviderTestBase
    {
        [Fact]
        public async Task GetNextChangesBatch_TasksWithOneWorkPathAndOptions_CacheIsWorking()
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
            var changesProvider = GetBlobImagesChangesProvider(blobContents);
            var thumbnailOption = new ThumbnailOption() { FileSuffix = OptionSuffix };
            var workPath = "testPath";
            var task1 = new ThumbnailTask()
            {
                Id = Guid.NewGuid().ToString(),
                LastRun = null,
                WorkPath = workPath,
                ThumbnailOptions = new List<ThumbnailOption>() { thumbnailOption },
            };
            var task2 = new ThumbnailTask()
            {
                Id = Guid.NewGuid().ToString(),
                LastRun = null,
                WorkPath = workPath,
                ThumbnailOptions = new List<ThumbnailOption>() { thumbnailOption },
            };
            var cancellationToken = new CancellationTokenWrapper(new CancellationToken());

            // Act
            var changes1 = await changesProvider.GetNextChangesBatch(task1, null, 0, 10, cancellationToken);
            var changes2 = await changesProvider.GetNextChangesBatch(task2, null, 0, 10, cancellationToken);

            //Assert
            StorageProviderMock.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            Assert.Single(changes1);
            Assert.Single(changes2);
            for (var i = 0; i < changes1.Length; i++)
            {
                Assert.Same(changes1[i], changes2[i]);
            }
        }

        [Fact]
        public async Task GetNextChangesBatch_TasksWithDifferentWorkPathAndOptions_UseDifferentValues()
        {
            // Arrange
            var blobContents = new List<BlobEntry>()
            {
                new BlobInfo()
                {
                    Name = "Blob1.png",
                    Url = "testPath1/Blob1.png",
                },
                new BlobInfo()
                {
                    Name = "Blob2.png",
                    Url = "testPath2/Blob2.png",
                },
            };
            var changesProvider = GetBlobImagesChangesProvider(blobContents);
            var thumbnailOption = new ThumbnailOption() { FileSuffix = OptionSuffix };
            var task1 = new ThumbnailTask()
            {
                Id = Guid.NewGuid().ToString(),
                LastRun = null,
                WorkPath = "testPath1",
                ThumbnailOptions = new List<ThumbnailOption>() { thumbnailOption },
            };
            var task2 = new ThumbnailTask()
            {
                Id = Guid.NewGuid().ToString(),
                LastRun = null,
                WorkPath = "testPath2",
                ThumbnailOptions = new List<ThumbnailOption>() { thumbnailOption },
            };
            var cancellationToken = new CancellationTokenWrapper(new CancellationToken());

            // Act
            var changes1 = await changesProvider.GetNextChangesBatch(task1, null, 0, 10, cancellationToken);
            var changes2 = await changesProvider.GetNextChangesBatch(task2, null, 0, 10, cancellationToken);

            //Assert
            StorageProviderMock.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));

            Assert.Collection(changes1, x => Assert.Equal("Blob1.png", x.Name));
            Assert.Collection(changes2, x => Assert.Equal("Blob2.png", x.Name));
        }

        [Fact]
        public async Task GetNextChangesBatch_TasksWithOneWorkPathAndOptions_CacheIsWorkingWithGetTotalChangesCount()
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
            var changesProvider = GetBlobImagesChangesProvider(blobContents);
            var thumbnailOption = new ThumbnailOption() { FileSuffix = OptionSuffix };
            var workPath = "testPath";
            var task = new ThumbnailTask()
            {
                Id = Guid.NewGuid().ToString(),
                LastRun = null,
                WorkPath = workPath,
                ThumbnailOptions = new List<ThumbnailOption>() { thumbnailOption },
            };
            var cancellationToken = new CancellationTokenWrapper(new CancellationToken());

            // Act
            var changes = await changesProvider.GetNextChangesBatch(task, null, 0, 10, cancellationToken);
            var count = await changesProvider.GetTotalChangesCount(task, null, cancellationToken);

            //Assert
            StorageProviderMock.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            Assert.Collection(changes, x => Assert.Equal("Blob1.png", x.Name));
            Assert.Equal(1, count);
        }
    }
}
