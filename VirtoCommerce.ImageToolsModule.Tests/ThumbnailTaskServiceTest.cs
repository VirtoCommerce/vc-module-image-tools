using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Moq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class ThumbnailTaskServiceTest
    {
        [Fact]
        public void Delete_ThumbnailOptionIds_RemoveThumbnailTasksByIdsCalled()
        {
            var id = ThumbnailTaskEntitysDataSource.ToList().FirstOrDefault().Id;
            var repoMock = GetTaskRepositoryMock();

            var serviceMock = new Mock<IThumbnailTaskService>();
            serviceMock.Setup(x => x.RemoveByIds(It.IsAny<string[]>())).Callback(
                (string[] ids) =>
                {
                    repoMock.Object.RemoveThumbnailTasksByIds(ids);
                });

            serviceMock.Object.RemoveByIds(new[] { id });
            repoMock.Verify(x => x.RemoveThumbnailTasksByIds(new[] { id }));
        }

        [Fact]
        public void GetByIds_ArrayOfIdis_ReturnsArrayOfThumbnailTasksWithPassedIds()
        {
            //var taskEntites = ThumbnailTaskEntitysDataSource.ToArray();

            //var ids = taskEntites.Select(t => t.Id).ToArray();
            //var tasks = taskEntites.Select(t => t.ToModel(new ThumbnailTask())).ToArray();

            //var mock = new Mock<IThumbnailRepository>();
            //mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>(ids)))
            //    .Returns(taskEntites.Where(t => ids.Contains(t.Id)).ToArray());

            //var sut = new ThumbnailTaskService(GetPerfectTaskRepositoryMock());
            //var result = sut.GetByIds(ids);

            //Assert.Equal(result, tasks);
        }

        //[Fact]
        //public void SaveChanges_ArrayOfThumbnailTasks_ThumbnailTasksSaved()
        //{
        //    var taskEntitys = new List<ThumbnailTaskEntity>();
        //    var tasks = ThumbnailTasksDataSource.ToArray();

        //    var mock = new Mock<IThumbnailRepository>();
        //    mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>())).Returns(
        //        (string[] ids) => { return taskEntitys.Where(t => ids.Contains(t.Id)).ToArray(); });

        //    var sut = new ThumbnailTaskService(() => mock.Object);
        //    sut.SaveOrUpdate(tasks);

        //    Assert.NotEmpty(taskEntitys);
        //    Assert.Equal(taskEntitys.Count, tasks.Length);
        //}

        //[Fact]
        //public void SaveChanges_ArrayOfThumbnailTasks_ThumbnailTasksWithOptionsUpdated()
        //{
        //    var taskEntitys = ThumbnailTaskEntitysDataSource.ToArray();
        //    var tasks = ThumbnailTasksDataSource.ToArray();

        //    var mock = new Mock<IThumbnailRepository>();
        //    mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>())).Returns(
        //        (string[] ids) => { return taskEntitys.Where(t => ids.Contains(t.Id)).ToArray(); });

        //    var sut = new ThumbnailTaskService(() => mock.Object);
        //    sut.SaveOrUpdate(tasks);

        //    Assert.Contains(taskEntitys, t => t.Name == "New Name");
        //    Assert.Contains(taskEntitys, t => t.WorkPath == "New Path");
        //    Assert.Contains(taskEntitys, t => t.ThumbnailTaskOptions == tasks.First().ThumbnailOptions);
        //}


        public Mock<IThumbnailRepository> GetTaskRepositoryMock()
        {
            var taskEntities = ThumbnailTaskEntitysDataSource.ToList();
            var tasksQuerableMock = TestUtils.CreateQuerableMock(taskEntities);
            var target = tasksQuerableMock.Object;

            var optionEntities = ThumbnailOptionDataSource.ToList();
            var optionsQuerableMock = TestUtils.CreateQuerableMock(optionEntities);

            var repoMock = new Mock<IThumbnailRepository>();
            repoMock.Setup(x => x.ThumbnailTasks).Returns(target);
            repoMock.Setup(x => x.ThumbnailOptions).Returns(optionsQuerableMock.Object);

            repoMock.Setup(x => x.GetThumbnailTasksByIds(It.IsAny<string[]>()))
                .Returns((string[] ids) => { return tasksQuerableMock.Object.Where(t => ids.Contains(t.Id)).ToArray(); });

            repoMock.Setup(x => x.RemoveThumbnailTasksByIds(It.IsAny<string[]>()))
                .Callback((string[] ids) =>
                {
                    target = target.Where(e => !ids.Contains(e.Id));
                });

            repoMock.Setup(x => x.GetThumbnailOptionsByIds(It.IsAny<string[]>()))
                .Returns((string[] ids) =>
                {
                    return optionsQuerableMock.Object.Where(t => ids.Contains(t.Id)).ToArray();
                });

            return repoMock;
        }

        public IEnumerable<ThumbnailTaskEntity> ThumbnailTaskEntitysDataSource
        {
            get
            {
                yield return new ThumbnailTaskEntity()
                {
                    Id = "Task1",
                    Name = "Name 1",
                    WorkPath = "Path 4",
                    ThumbnailTaskOptions = new ObservableCollection<ThumbnailTaskOptionEntity>
                {
                    new ThumbnailTaskOptionEntity() { Id = "1", ThumbnailOptionId = "Option1", ThumbnailTaskId = "Task1" },
                    new ThumbnailTaskOptionEntity { Id = "2", ThumbnailOptionId = "Option2", ThumbnailTaskId = "Task1" }
                }
                };
                yield return new ThumbnailTaskEntity()
                {
                    Id = "Task2",
                    Name = "NameLong 2",
                    WorkPath = "Path 3",
                    ThumbnailTaskOptions = new ObservableCollection<ThumbnailTaskOptionEntity>
                    {
                        new ThumbnailTaskOptionEntity() { Id = "3", ThumbnailOptionId = "Option3", ThumbnailTaskId = "Task2" },
                        new ThumbnailTaskOptionEntity { Id = "4", ThumbnailOptionId = "Option4", ThumbnailTaskId = "Task2" }
                    }
                };
            }
        }

        private IEnumerable<ThumbnailOptionEntity> ThumbnailOptionDataSource
        {
            get
            {
                yield return new ThumbnailOptionEntity { Id = $"Option1", Name = "Option Name 1" };
                yield return new ThumbnailOptionEntity { Id = $"Option2", Name = "Option Name 2" };
                yield return new ThumbnailOptionEntity { Id = $"Option3", Name = "Option Name 3" };
                yield return new ThumbnailOptionEntity { Id = $"Option4", Name = "Option Name 4" };
            }
        }

    }
}