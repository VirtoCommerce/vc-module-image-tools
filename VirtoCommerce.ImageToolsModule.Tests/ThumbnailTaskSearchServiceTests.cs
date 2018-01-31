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
    public class ThumbnailTaskSearchServiceTests
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
        public void SerchTasks_ThumbnailOptionSearchCriteria_ReturnsGenericSearchResponseOfTasksInExpectedOrder()
        {
            var taskEntitys = ThumbnailTaskEntitysDataSource.ToArray();
            var expectedTasks = ThumbnailTasksDataSource.OrderBy(t => t.Name).ThenByDescending(t => t.WorkPath).ToArray();

            var criteria = new ThumbnailOptionSearchCriteria {Sort = "Name:asc;WorkPath:desc"};

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>()))
                .Returns((string[] ids) => { return taskEntitys.Where(t => ids.Contains(t.Id)).ToArray(); });

            var sut = new ThumbnailTaskSearchService(mock.Object);

            var resultTasks = sut.SerchTasks(criteria);
            
            Assert.Equal(resultTasks, expectedTasks, new ThumbnailTaskEntityComparer());
        }
        
        [Fact]
        public void SerchTasks_KeywordString_ReturnsKeywordMatchingGenericSearchResponseOfTasks()
        {
            var taskEntitys = ThumbnailTaskEntitysDataSource.ToArray();
            var expectedTasks = ThumbnailTasksDataSource.OrderBy(t => t.Name).ThenByDescending(t => t.WorkPath).ToArray();

            var keyword = "New Name";

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>()))
                .Returns((string[] ids) => { return taskEntitys.Where(t => ids.Contains(t.Id)).ToArray(); });

            var sut = new ThumbnailTaskSearchService(mock.Object);

            var resultTasks = sut.SerchTasks(keyword);
            
            Assert.Equal(resultTasks, expectedTasks, new ThumbnailTaskEntityComparer());
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
                int i = 0;
                yield return new ThumbnailTask() {Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path"};
                yield return new ThumbnailTask() {Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path"};
                yield return new ThumbnailTask() {Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path"};
            }
        }

    }
}