using Moq;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class ThumbnailTaskSearchServiceTests
    {
        [Fact]
        public void SerchTasks_ThumbnailOptionSearchCriteria_ReturnsGenericSearchResponseOfTasksInExpectedOrder()
        {
            var taskEntitys = ThumbnailTaskEntitysDataSource.ToArray();
            var expectedTasks = ThumbnailTasksDataSource.OrderBy(t => t.Name).ThenByDescending(t => t.WorkPath).ToArray();

            var criteria = new ThumbnailTaskSearchCriteria { Sort = "Name:asc;WorkPath:desc" };

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>()))
                .Returns((string[] ids) => { return taskEntitys.Where(t => ids.Contains(t.Id)).ToArray(); });

            var sut = new ThumbnailTaskSearchService(() => mock.Object);

            var resultTasks = sut.SerchTasks(criteria);

            Assert.Equal(expectedTasks, resultTasks.Results);
        }

        [Fact]
        public void SerchTasks_KeywordString_ReturnsKeywordMatchingGenericSearchResponseOfTasks()
        {
            var keyword = "New Name";
            var taskEntites = ThumbnailTaskEntitysDataSource.ToArray();
            var expectedTasks = ThumbnailTasksDataSource.Where(t => t.Name == keyword).ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailTasksByIds(It.IsIn<string[]>()))
                .Returns((string[] ids) => { return taskEntites.Where(t => ids.Contains(t.Id)).ToArray(); });

            var sut = new ThumbnailTaskSearchService(() => mock.Object);

            var resultTasks = sut.SerchTasks(keyword);

            Assert.Equal(expectedTasks, resultTasks.Results);
        }

        private static IEnumerable<ThumbnailTaskEntity> ThumbnailTaskEntitysDataSource
        {
            get
            {
                int i = 0;
                yield return new ThumbnailTaskEntity() { Id = $"Task {++i}" };
                yield return new ThumbnailTaskEntity() { Id = $"Task {++i}" };
                yield return new ThumbnailTaskEntity() { Id = $"Task {++i}" };
            }
        }

        private static IEnumerable<ThumbnailTask> ThumbnailTasksDataSource
        {
            get
            {
                int i = 0;
                yield return new ThumbnailTask() { Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path" };
                yield return new ThumbnailTask() { Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path" };
                yield return new ThumbnailTask() { Id = $"Task {++i}", Name = "New Name", WorkPath = "New Path" };
            }
        }
    }
}