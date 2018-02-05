namespace VirtoCommerce.ImageToolsModule.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Moq;

    using VirtoCommerce.ImageToolsModule.Core.Models;
    using VirtoCommerce.ImageToolsModule.Data.Models;
    using VirtoCommerce.ImageToolsModule.Data.Repositories;
    using VirtoCommerce.ImageToolsModule.Data.Services;

    using Xunit;

    public class ThumbnailTaskServiceTest
    {
        private static IEnumerable<ThumbnailOption> ThumbnailOptionDataSource
        {
            get
            {
                var i = 0;
                yield return new ThumbnailOption { Id = $"Option {++i}", Name = "New Name" };
                yield return new ThumbnailOption { Id = $"Option {++i}", Name = "New Name" };
                yield return new ThumbnailOption { Id = $"Option {++i}", Name = "New Name" };
            }
        }

        private static IEnumerable<ThumbnailTaskEntity> ThumbnailTaskEntitysDataSource
        {
            get
            {
                var i = 0;
                yield return new ThumbnailTaskEntity { Id = $"Task {++i}" };
                yield return new ThumbnailTaskEntity { Id = $"Task {++i}" };
                yield return new ThumbnailTaskEntity { Id = $"Task {++i}" };
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

        [Fact]
        public void Delete_ThumbnailOptionIds_DeletedThumbnailTasksWithPassedIds()
        {
            var taskEntites = ThumbnailTaskEntitysDataSource.ToList();

            var ids = taskEntites.Select(t => t.Id).ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.RemoveThumbnailTasksByIds(It.IsIn<string[]>(ids))).Callback(
                (string[] arr) =>
                    {
                        var entities = taskEntites.Where(e => arr.Contains(e.Id));
                        foreach (var entity in entities) taskEntites.Remove(entity);
                    });

            var sut = new ThumbnailTaskService(() => mock.Object);
            sut.RemoveByIds(ids);

            Assert.Empty(taskEntites);
        }

        [Fact]
        public void GetByIds_ArrayOfIdis_ReturnsArrayOfThumbnailTasksWithPassedIds()
        {
            var taskEntites = ThumbnailTaskEntitysDataSource.ToArray();

            var ids = taskEntites.Select(t => t.Id).ToArray();
            var tasks = taskEntites.Select(t => t.ToModel(new ThumbnailTask())).ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>(ids)))
                .Returns(taskEntites.Where(t => ids.Contains(t.Id)).ToArray());

            var sut = new ThumbnailTaskService(() => mock.Object);
            var result = sut.GetByIds(ids);

            Assert.Equal(result, tasks);
        }

        [Fact]
        public void SaveChanges_ArrayOfThumbnailTasks_ThumbnailTasksSaved()
        {
            var taskEntitys = new List<ThumbnailTaskEntity>();
            var tasks = ThumbnailTasksDataSource.ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>())).Returns(
                (string[] ids) => { return taskEntitys.Where(t => ids.Contains(t.Id)).ToArray(); });

            var sut = new ThumbnailTaskService(() => mock.Object);
            sut.SaveOrUpdate(tasks);

            Assert.NotEmpty(taskEntitys);
            Assert.Equal(taskEntitys.Count, tasks.Length);
        }

        [Fact]
        public void SaveChanges_ArrayOfThumbnailTasks_ThumbnailTasksWithOptionsUpdated()
        {
            var taskEntitys = ThumbnailTaskEntitysDataSource.ToArray();
            var tasks = ThumbnailTasksDataSource.ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>())).Returns(
                (string[] ids) => { return taskEntitys.Where(t => ids.Contains(t.Id)).ToArray(); });

            var sut = new ThumbnailTaskService(() => mock.Object);
            sut.SaveOrUpdate(tasks);

            Assert.Contains(taskEntitys, t => t.Name == "New Name");
            Assert.Contains(taskEntitys, t => t.WorkPath == "New Path");
            Assert.Contains(taskEntitys, t => t.ThumbnailTaskOptionEntities == tasks.First().ThumbnailOptions);
        }
    }
}