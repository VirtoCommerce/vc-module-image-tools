using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    [TestFixture]
    public class ThumbnailTaskServiceTest
    {
        [Test]
        public void GetByIds_ArrayOfIdis_ReturnsArrayOfThumbnailTasksWithPassedIds()
        {
            var taskEntites = new ThumbnailTaskEntity[10];

            for (int i = 0; i < taskEntites.Length; i++)
            {
                var task = new ThumbnailTaskEntity();
                task.Id = Guid.NewGuid().ToString();
                taskEntites[i] = task;
            }

            var ids = taskEntites.Select(t => t.Id).ToArray();
            var tasks = taskEntites.Select(t => t.ToModel(new ThumbnailTask())).ToArray();
            
            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>(ids))).Returns(taskEntites);

            var sut = new ThumbnailTaskService(mock.Object);
            var result = sut.GetByIds(ids);
            
            Assert.That(result, Is.EqualTo(tasks));
        }

        [Test]
        public void Delete_ThumbnailOptionIds_DeletedThumbnailTasksWithPassedIds()
        {
            var taskEntites = new List<ThumbnailTaskEntity>();

            for (int i = 0; i < 10; i++)
            {
                var task = new ThumbnailTaskEntity();
                task.Id = Guid.NewGuid().ToString();
                taskEntites.Add(task);
            }

            var ids = taskEntites.Select(t => t.Id).ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.DeletedThumbnailTasksByIds(It.IsIn<string[]>(ids)))
                .Callback((string[] arr) =>
                {
                    var entities = taskEntites.Where(e => arr.Contains(e.Id));
                    foreach (var entity in entities)
                    {
                        taskEntites.Remove(entity);
                    }
                } );

            var sut = new ThumbnailTaskService(mock.Object);
            sut.DeleteByIds(ids);
            
            Assert.That(taskEntites, Is.Empty);            
        }
    }
}