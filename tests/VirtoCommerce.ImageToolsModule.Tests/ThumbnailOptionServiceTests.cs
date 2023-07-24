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
    public class ThumbnailOptionServiceTests
    {
        [Fact]
        public async Task GetByIds_ArrayOfIds_ReturnsArrayOfThumbnailOption()
        {
            var entities = ThumbnailOptionEntitiesDataSource.ToList();
            var ids = entities.Select(t => t.Id).ToList();
            var expectedModels = entities.Select(t => t.ToModel(new ThumbnailOption())).ToList();
            var service = GetThumbnailOptionService(entities);

            var result = await service.GetAsync(ids);

            Assert.Equal(expectedModels, result);
        }

        [Fact]
        public async Task Delete_ThumbnailOptionIds_DeletedThumbnailOptionWithPassedIds()
        {
            var entities = ThumbnailOptionEntitiesDataSource.ToList();
            var ids = entities.Select(t => t.Id).ToList();
            var service = GetThumbnailOptionService(entities);

            await service.DeleteAsync(ids);

            Assert.Empty(entities);
        }

        [Fact]
        public async Task SaveChanges_ArrayOfThumbnailOptions_ThumbnailOptionsUpdatedAsync()
        {
            var entities = ThumbnailOptionEntitiesDataSource.ToList();
            var options = ThumbnailOptionDataSource.ToList();
            var service = GetThumbnailOptionService(entities);

            await service.SaveChangesAsync(options);

            Assert.Contains(entities, o => o.Name == "New Name");
        }

        [Fact]
        public async Task SaveChanges_ArrayOfThumbnailOptions_NewThumbnailOptionsSaved()
        {
            var entities = ThumbnailOptionEntitiesDataSource.ToList();
            var service = GetThumbnailOptionService(entities);

            await service.SaveChangesAsync(new[]
            {
                new ThumbnailOption
                {
                    Id = "NewOptionId", Name = "New Option name"
                }
            });

            Assert.Contains(entities, x => x.Id == "NewOptionId");
        }


        private static IEnumerable<ThumbnailOptionEntity> ThumbnailOptionEntitiesDataSource
        {
            get
            {
                yield return new ThumbnailOptionEntity { Id = "Option 1" };
                yield return new ThumbnailOptionEntity { Id = "Option 2" };
                yield return new ThumbnailOptionEntity { Id = "Option 3" };
            }
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

        private static IThumbnailOptionService GetThumbnailOptionService(IList<ThumbnailOptionEntity> entities)
        {
            var repositoryMock = new Mock<IThumbnailRepository>();

            repositoryMock
                .SetupGet(x => x.UnitOfWork)
                .Returns(new Mock<IUnitOfWork>().Object);

            repositoryMock
                .Setup(r => r.GetThumbnailOptionsByIdsAsync(It.IsAny<IList<string>>()))
                .ReturnsAsync((IList<string> ids) => { return entities.Where(t => ids.Contains(t.Id)).ToList(); });

            repositoryMock
                .Setup(x => x.Add(It.IsAny<ThumbnailOptionEntity>()))
                .Callback((ThumbnailOptionEntity entity) => { entities.Add(entity); });

            repositoryMock
                .Setup(x => x.Remove(It.IsAny<ThumbnailOptionEntity>()))
                .Callback((ThumbnailOptionEntity entity) => { entities.Remove(entity); });

            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

            return new ThumbnailOptionService(() => repositoryMock.Object, platformMemoryCache, new Mock<IEventPublisher>().Object);
        }
    }
}
