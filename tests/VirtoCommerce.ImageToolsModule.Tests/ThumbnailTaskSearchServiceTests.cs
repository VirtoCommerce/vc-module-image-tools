using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.GenericCrud;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class ThumbnailTaskSearchServiceTests
    {
        [Fact]
        public async Task Search_ThumbnailOptionSearchCriteria_ReturnsGenericSearchResponseOfTasksInExpectedOrder()
        {
            // Arrange
            var service = GetThumbnailTaskSearchService(ThumbnailTaskEntitiesDataSource.ToList());
            var criteria = new ThumbnailTaskSearchCriteria { Sort = "Name:desc;WorkPath:desc" };
            var expectedModels = ThumbnailTaskEntitiesDataSource.Select(x => x.ToModel(new ThumbnailTask())).OrderByDescending(t => t.Name).ThenByDescending(t => t.WorkPath).ToArray();

            // Act
            var searchResult = await service.SearchAsync(criteria);

            // Assert
            Assert.Equal(expectedModels, searchResult.Results);
        }

        [Fact]
        public async Task Search_SearchByExistingKeyword_TasksFound()
        {
            // Arrange
            var service = GetThumbnailTaskSearchService(ThumbnailTaskEntitiesDataSource.ToList());
            var keyword = "NameLong";
            var expectedCount = ThumbnailTaskEntitiesDataSource.Count(x => x.Name.Contains(keyword));

            // Act
            var searchResult = await service.SearchAsync(new ThumbnailTaskSearchCriteria { Keyword = keyword });

            // Assert
            Assert.Equal(expectedCount, searchResult.Results.Count);
        }


        private static IEnumerable<ThumbnailTaskEntity> ThumbnailTaskEntitiesDataSource
        {
            get
            {
                yield return new ThumbnailTaskEntity { Id = "Task1", Name = "Name 1", WorkPath = "Path 4" };
                yield return new ThumbnailTaskEntity { Id = "Task2", Name = "NameLong 2", WorkPath = "Path 3" };
                yield return new ThumbnailTaskEntity { Id = "Task3", Name = "Name 3", WorkPath = "Path 2" };
                yield return new ThumbnailTaskEntity { Id = "Task4", Name = "NameLong 4", WorkPath = "Path 1" };
            }
        }

        private static IThumbnailTaskSearchService GetThumbnailTaskSearchService(IList<ThumbnailTaskEntity> entities)
        {
            var repositoryMock = new Mock<IThumbnailRepository>();

            repositoryMock
                .Setup(x => x.ThumbnailTasks)
                .Returns(entities.AsQueryable().BuildMock());

            repositoryMock
                .Setup(x => x.GetThumbnailTasksByIdsAsync(It.IsAny<IList<string>>()))
                .ReturnsAsync((IList<string> ids) => { return entities.Where(t => ids.Contains(t.Id)).ToList(); });

            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

            var crudService = new ThumbnailTaskService(() => repositoryMock.Object, platformMemoryCache, new Mock<IEventPublisher>().Object);

            return new ThumbnailTaskSearchService(() => repositoryMock.Object, platformMemoryCache, crudService, Options.Create(new CrudOptions()));
        }
    }
}
