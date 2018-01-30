using System.Runtime.InteropServices;
 using System.Threading;
 using System.Threading.Tasks;
 using NUnit.Framework;
 using VirtoCommerce.ImageToolsModule.Core.Models;
 using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
 using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
 
 namespace VirtoCommerce.ImageToolsModule.Tests
 {
     public class DefaultThumbnailGeneratorTests
     {
         private DefaultThumbnailGenerator _thumbnailGenerator;
         
         [SetUp]
         public void Init()
         {
             _thumbnailGenerator = new DefaultThumbnailGenerator();
         }
 
         [Test]
         public async void GenerateThumbnailsAsync_ValidValues_ReturnsNotEmptyListOfThumbnails()
         {
             var sourse = "";
             var destination = "";
             var option = new ThumbnailOption();
             var source = new CancellationTokenSource();
             var token = source.Token;
 
             var result =  await _thumbnailGenerator.GenerateThumbnailsAsync(sourse, destination, option, token);
 
             Assert.That(result.GeneratedThumbnails, Is.Not.Empty);
         }
     }
 }