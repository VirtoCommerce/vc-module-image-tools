using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class ThumbnailTaskServiceTest
    {     
        [Fact]
        public void Delete_ThumbnailOptionIds_DeletedThumbnailTasksWithPassedIds()
        {
            var taskEntities = ThumbnailTaskEntitysDataSource.ToList();

            var ids = taskEntities.Select(t => t.Id).ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.SetupGet(x => x.UnitOfWork).Returns(new Mock<IUnitOfWork>().Object);
            mock.Setup(r => r.RemoveThumbnailTasksByIds(It.IsIn<string[]>(ids))).Callback(
                (string[] arr) =>
                {
                    var entities = taskEntities.Where(e => arr.Contains(e.Id)).ToList();
                    foreach (var entity in entities)
                    {
                        taskEntities.Remove(entity);
                    }
                });

            var sut = new ThumbnailTaskService(() => mock.Object);
            sut.RemoveByIds(ids);

            Assert.Empty(taskEntities);
        }

        [Fact]
        public void GetByIds_ArrayOfIdis_ReturnsArrayOfThumbnailTasksWithPassedIds()
        {
            var taskEntities = ThumbnailTaskEntitysDataSource.ToArray();

            var ids = taskEntities.Select(t => t.Id).ToArray();
            var tasks = taskEntities.Select(t => t.ToModel(new ThumbnailTask())).ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.SetupGet(x => x.UnitOfWork).Returns(new Mock<IUnitOfWork>().Object);
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>(ids)))
                .Returns(taskEntities.Where(t => ids.Contains(t.Id)).ToArray());

            var sut = new ThumbnailTaskService(() => mock.Object);
            var result = sut.GetByIds(ids);

            Assert.Equal(result, tasks);
        }

        [Fact]
        public void SaveChanges_ArrayOfThumbnailTasks_ThumbnailTasksSaved()
        {
            var taskEntities = new List<ThumbnailTaskEntity>();

            var mock = new Mock<IThumbnailRepository>();
            mock.SetupGet(x => x.UnitOfWork).Returns(new Mock<IUnitOfWork>().Object);
            mock.Setup(x => x.Add(It.IsAny<ThumbnailTaskEntity>()))
                .Callback((ThumbnailTaskEntity entity) =>
                {
                    taskEntities.Add(entity);
                });
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsAny<string[]>())).Returns(
                (string[] ids) => { return taskEntities.Where(t => ids.Contains(t.Id)).ToArray(); });

            var sut = new ThumbnailTaskService(() => mock.Object);
            sut.SaveOrUpdate(new[]
            {
                new ThumbnailTask()
                {
                    Id = "NewTaskId"
                }
            });

            Assert.Contains(taskEntities, x=>x.Id == "NewTaskId");
        }

        private static IEnumerable<ThumbnailOption> ThumbnailOptionDataSource
        {
            get
            {
                yield return new ThumbnailOption { Id = "Option 1", Name = "New Name" };
                yield return new ThumbnailOption { Id = "Option 2", Name = "New Name" };
                yield return new ThumbnailOption { Id = "Option 3", Name = "New Name" };
            }
        }

        private static IEnumerable<ThumbnailTaskEntity> ThumbnailTaskEntitysDataSource
        {
            get
            {
                yield return new ThumbnailTaskEntity { Id = "Task 1" };
                yield return new ThumbnailTaskEntity { Id = "Task 2" };
                yield return new ThumbnailTaskEntity { Id = "Task 3" };
            }
        }

        private static IEnumerable<ThumbnailTask> ThumbnailTasksDataSource
        {
            get
            {
                var options = ThumbnailOptionDataSource.ToList();

                var i = 0;
                yield return new ThumbnailTask
                {
                    Id = $"Task {++i}",
                    Name = "New Name",
                    WorkPath = "New Path",
                    ThumbnailOptions = options
                };
                yield return new ThumbnailTask
                {
                    Id = $"Task {++i}",
                    Name = "New Name",
                    WorkPath = "New Path",
                    ThumbnailOptions = options
                };
                yield return new ThumbnailTask
                {
                    Id = $"Task {++i}",
                    Name = "New Name",
                    WorkPath = "New Path",
                    ThumbnailOptions = options
                };
            }
        }
    }
}