using System.IO;
using System.Threading;
using Moq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Assets;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
 {
     public class DefaultThumbnailGeneratorTests
     {
         [Fact]
         public async void GenerateThumbnailsAsync_ValidValues_ReturnsNotEmptyListOfThumbnails()
         {
             //var option = new ThumbnailOption();
             
             //var source = new CancellationTokenSource();
             //var token = source.Token;
             
             //var inStream = new MemoryStream();
             //var outStream = new MemoryStream();
             
             //var mock = new Mock<IBlobStorageProvider>();
             //mock.Setup(r => r.OpenRead(It.IsAny<string>())).Returns(() => inStream);
             //mock.Setup(r => r.OpenWrite(It.IsAny<string>())).Returns(() => outStream);

             //var sut = new DefaultThumbnailGenerator(mock.Object);
             //var result =  await sut.GenerateThumbnailsAsync("src", "dest", option, token);
 
             //Assert.NotEmpty(result.GeneratedThumbnails);
         }
     }
 }