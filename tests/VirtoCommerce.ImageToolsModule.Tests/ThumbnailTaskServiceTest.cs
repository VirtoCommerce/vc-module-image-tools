using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class ThumbnailTaskServiceTest
    {
        [Fact]
        public async Task Delete_ThumbnailOptionIds_DeletedThumbnailTasksWithPassedIds()
        {
            var entities = ThumbnailTaskEntityDataSource.ToList();
            var ids = entities.Select(t => t.Id).ToArray();
            var service = GetThumbnailTaskService(entities);

            await service.DeleteAsync(ids);

            Assert.Empty(entities);
        }

        [Fact]
        public async Task GetByIds_ArrayOfIds_ReturnsArrayOfThumbnailTasksWithPassedIds()
        {
            var entities = ThumbnailTaskEntityDataSource.ToArray();
            var ids = entities.Select(t => t.Id).ToArray();
            var expectedModels = entities.Select(t => t.ToModel(new ThumbnailTask())).ToArray();
            var service = GetThumbnailTaskService(entities);

            var result = await service.GetAsync(ids);

            Assert.Equal(expectedModels, result);
        }

        [Fact]
        public async Task SaveChanges_ArrayOfThumbnailTasks_ThumbnailTasksSaved()
        {
            var entities = new List<ThumbnailTaskEntity>();
            var service = GetThumbnailTaskService(entities);

            await service.SaveChangesAsync(new[]
            {
                new ThumbnailTask
                {
                    Id = "NewTaskId"
                }
            });

            Assert.Contains(entities, x => x.Id == "NewTaskId");
        }


        private static IEnumerable<ThumbnailTaskEntity> ThumbnailTaskEntityDataSource
        {
            get
            {
                yield return new ThumbnailTaskEntity { Id = "Task 1" };
                yield return new ThumbnailTaskEntity { Id = "Task 2" };
                yield return new ThumbnailTaskEntity { Id = "Task 3" };
            }
        }

        private static IThumbnailTaskService GetThumbnailTaskService(IList<ThumbnailTaskEntity> entities)
        {
            var repositoryMock = new Mock<IThumbnailRepository>();

            repositoryMock
                .SetupGet(x => x.UnitOfWork)
                .Returns(new Mock<IUnitOfWork>().Object);

            repositoryMock
                .Setup(r => r.GetThumbnailTasksByIdsAsync(It.IsAny<IList<string>>()))
                .ReturnsAsync((IList<string> ids) => { return entities.Where(t => ids.Contains(t.Id)).ToList(); });

            repositoryMock
                .Setup(x => x.Add(It.IsAny<ThumbnailTaskEntity>()))
                .Callback((ThumbnailTaskEntity entity) => { entities.Add(entity); });

            repositoryMock
                .Setup(x => x.Remove(It.IsAny<ThumbnailTaskEntity>()))
                .Callback((ThumbnailTaskEntity entity) => { entities.Remove(entity); });

            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

            return new ThumbnailTaskService(() => repositoryMock.Object, platformMemoryCache, new Mock<IEventPublisher>().Object);
        }
    }
}
