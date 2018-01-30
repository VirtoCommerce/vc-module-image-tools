using System.Threading;
using Moq;
using NUnit.Framework;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    [TestFixture]
    public class ThumbnailGenerationProcessorTest
    {
        [Test]
        public void ProcessTasksAsync_ValidValues_Returns()
        {
            var mock = new Mock<IThumbnailGenerator>();
            mock.Setup(g => g.GenerateThumbnailsAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<ThumbnailOption>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((sourcePath, destPath, option, ctoken) =>
                {
                    
                });

            var tasks = new ThumbnailTask[10];
            
            var source = new CancellationTokenSource();
            var token = source.Token;
            
            var sud = new ThumbnailGenerationProcessor(mock.Object);
            sud.ProcessTasksAsync( ;

        }
    }
}