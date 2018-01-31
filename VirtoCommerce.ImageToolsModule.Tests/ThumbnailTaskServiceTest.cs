using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class ThumbnailTaskServiceTest
    {
        private class ThumbnailTaskEntityComparer : IEqualityComparer<ThumbnailTaskEntity>
        {
            public bool Equals(ThumbnailTaskEntity x, ThumbnailTaskEntity y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(ThumbnailTaskEntity obj)
            {
                return obj.GetHashCode();
            }
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

            var sut = new ThumbnailTaskService(mock.Object);
            var result = sut.GetByIds(ids);
            
            Assert.Equal(result, tasks, new ThumbnailTaskEntityComparer());
        }

        [Fact]
        public void Delete_ThumbnailOptionIds_DeletedThumbnailTasksWithPassedIds()
        {
            var taskEntites = ThumbnailTaskEntitysDataSource.ToList();

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
            
            Assert.Empty(taskEntites);
        }

        [Fact]
        public void SaveChanges_ArrayOfThumbnailTasks_ThumbnailTasksWithOptionsUpdated()
        {
            var taskEntitys = ThumbnailTaskEntitysDataSource.ToArray();
            var tasks = ThumbnailTasksDataSource.ToArray();
            
            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>()))
                .Returns((string[] ids) => { return taskEntitys.Where(t => ids.Contains(t.Id)).ToArray(); });
            
            var sut = new ThumbnailTaskService(mock.Object);
            sut.SaveChanges(tasks);
            
            Assert.Contains(taskEntitys, t => t.Name == "New Name");
            Assert.Contains(taskEntitys, t => t.WorkPath == "New Path");
            Assert.Contains(taskEntitys, t => t.ThumbnailTaskOptions == tasks.First().ThumbnailOptions);
        }
        
        [Fact]
        public void SaveChanges_ArrayOfThumbnailTasks_ThumbnailTasksSaved()
        {
            var taskEntitys = new List<ThumbnailTaskEntity>();
            var tasks = ThumbnailTasksDataSource.ToArray();
            
            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>()))
                .Returns((string[] ids) => { return taskEntitys.Where(t => ids.Contains(t.Id)).ToArray(); });
            
            var sut = new ThumbnailTaskService(mock.Object);
            sut.SaveChanges(tasks);
            
            Assert.NotEmpty(taskEntitys);
            Assert.Equal(taskEntitys.Count, tasks.Length);
        }
        
        private static IEnumerable<ThumbnailTaskEntity> ThumbnailTaskEntitysDataSource
        {
            get
            {
                int i = 0;
                yield return new ThumbnailTaskEntity() {Id = $"Task {++i}"};
                yield return new ThumbnailTaskEntity() {Id = $"Task {++i}"};
                yield return new ThumbnailTaskEntity() {Id = $"Task {++i}"};
            }
        }
        
        private static IEnumerable<ThumbnailTask> ThumbnailTasksDataSource
        {
            get
            {
                var options = ThumbnailOptionDataSource.ToList();
                
                int i = 0;
                yield return new ThumbnailTask() {Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path", ThumbnailOptions = options};
                yield return new ThumbnailTask() {Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path", ThumbnailOptions = options};
                yield return new ThumbnailTask() {Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path", ThumbnailOptions = options};
            }
        }
        
        private static IEnumerable<ThumbnailOption> ThumbnailOptionDataSource
        {
            get
            {
                int i = 0;
                yield return new ThumbnailOption() {Id = $"Option {++i}", Name = "New Name"};
                yield return new ThumbnailOption() {Id = $"Option {++i}", Name = "New Name"};
                yield return new ThumbnailOption() {Id = $"Option {++i}", Name = "New Name"};
            }
        }
    }
}