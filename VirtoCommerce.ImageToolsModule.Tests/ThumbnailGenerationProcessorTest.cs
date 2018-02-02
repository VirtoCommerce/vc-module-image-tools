using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class ThumbnailGenerationProcessorTest
    {
        [Fact]
        public async Task ProcessTasksAsync_ValidValues_CallbackFunctionHasBeenInvoked()
        {
            var mock = new Mock<IThumbnailGenerator>();

            mock.Setup(g => g.GenerateThumbnailsAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<ThumbnailOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ThumbnailGenerationResult());

            var tasks = ThumbnailTasksDataSource.ToArray();

            var source = new CancellationTokenSource();
            var token = source.Token;

            var called = false;

            var sud = new ThumbnailGenerationProcessor(mock.Object);
            await sud.ProcessTasksAsync(tasks, p => called = true, token);

            Assert.Equal(true, called);
        }

        [Fact]
        public async Task ProcessTasksAsync_ValidValues_GenerationProcessWasInterrupted()
        {
            var genResult = new ThumbnailGenerationResult();

            var mock = new Mock<IThumbnailGenerator>();
            mock.Setup(g => g.GenerateThumbnailsAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<ThumbnailOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string src, string dest, ThumbnailOption option, CancellationToken token) =>
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (token.IsCancellationRequested) return genResult;

                        Thread.Sleep(100);
                    }

                    genResult.GeneratedThumbnails.AddRange(new[] { "Nail 1", "Nail 2", "Nail 3" });
                    return genResult;
                });

            var tasks = ThumbnailTasksDataSource.ToArray();
            var source = new CancellationTokenSource();
            var sud = new ThumbnailGenerationProcessor(mock.Object);

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                source.Cancel();
            });

            await sud.ProcessTasksAsync(tasks, p => { }, source.Token);

            Assert.Empty(genResult.GeneratedThumbnails);
        }

        private static IEnumerable<ThumbnailTask> ThumbnailTasksDataSource
        {
            get
            {
                int i = 0;
                yield return new ThumbnailTask() { Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path" };
                yield return new ThumbnailTask() { Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path" };
                yield return new ThumbnailTask() { Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path" };
            }
        }
    }
}