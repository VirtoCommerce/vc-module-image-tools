using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    [TestFixture]
    public class ThumbnailOptionSearchServiceTests
    {
        [Test]
        public void GetByIds_ArrayOfIdis_ReturnsArrayOfThumbnailOption()
        {
            var optionEntites = new ThumbnailOptionEntity[10];

            for (int i = 0; i < optionEntites.Length; i++)
            {
                var task = new ThumbnailOptionEntity();
                task.Id = Guid.NewGuid().ToString();
                optionEntites[i] = task;
            }

            var ids = optionEntites.Select(t => t.Id).ToArray();
            var tasks = optionEntites.Select(t => t.ToModel(new ThumbnailOption())).ToArray();
            
            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailOptionsByIds(It.IsIn<string[]>(ids))).Returns(optionEntites);

            var sut = new ThumbnailTaskService(mock.Object);
            var result = sut.GetByIds(ids);
            
            Assert.That(result, Is.EqualTo(tasks));
        }

        [Test]
        public void Delete_ThumbnailOptionIds_DeletedThumbnailOptionWithPassedIds()
        {
            var optionEntites = new List<ThumbnailOptionEntity>();

            for (int i = 0; i < 10; i++)
            {
                var task = new ThumbnailOptionEntity();
                task.Id = Guid.NewGuid().ToString();
                optionEntites.Add(task);
            }

            var ids = optionEntites.Select(t => t.Id).ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.DeletedThumbnailOptionsByIds(It.IsIn<string[]>(ids)))
                .Callback((string[] arr) =>
                {
                    var entities = optionEntites.Where(e => arr.Contains(e.Id));
                    foreach (var entity in entities)
                    {
                        optionEntites.Remove(entity);
                    }
                } );

            var sut = new ThumbnailTaskService(mock.Object);
            sut.DeleteByIds(ids);
            
            Assert.That(optionEntites, Is.Empty);    
        }
    }
}