using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable;
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
    public class ThumbnailOptionSearchServiceTests
    {
        [Fact]
        public async Task Search_ThumbnailOptionSearchCriteria_ReturnsGenericSearchResponseOfTasksInExpectedOrder()
        {
            var service = GetThumbnailOptionSearchService(ThumbnailOptionEntitiesDataSource.ToList());
            var criteria = new ThumbnailOptionSearchCriteria { Sort = "Name:desc;FileSuffix:desc" };
            var expectedModels = ThumbnailOptionEntitiesDataSource.Select(x => x.ToModel(new ThumbnailOption())).OrderByDescending(t => t.Name).ThenByDescending(t => t.FileSuffix).ToList();

            var searchResult = await service.SearchAsync(criteria);

            Assert.Equal(expectedModels, searchResult.Results);
        }


        private static IEnumerable<ThumbnailOptionEntity> ThumbnailOptionEntitiesDataSource
        {
            get
            {
                yield return new ThumbnailOptionEntity { Id = "Option1", Name = "Name 1", FileSuffix = "SuffixName4" };
                yield return new ThumbnailOptionEntity { Id = "Option2", Name = "NameLong 2", FileSuffix = "SuffixName3" };
                yield return new ThumbnailOptionEntity { Id = "Option3", Name = "Name 3", FileSuffix = "SuffixName2" };
                yield return new ThumbnailOptionEntity { Id = "Option4", Name = "NameLong 4", FileSuffix = "SuffixName1" };
            }
        }

        private static IThumbnailOptionSearchService GetThumbnailOptionSearchService(IList<ThumbnailOptionEntity> entities)
        {
            var repositoryMock = new Mock<IThumbnailRepository>();

            repositoryMock
                .Setup(x => x.ThumbnailOptions)
                .Returns(entities.AsQueryable().BuildMock());

            repositoryMock
                .Setup(r => r.GetThumbnailOptionsByIdsAsync(It.IsAny<IList<string>>()))
                .ReturnsAsync((IList<string> ids) => { return entities.Where(t => ids.Contains(t.Id)).ToList(); });

            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

            var crudService = new ThumbnailOptionService(() => repositoryMock.Object, platformMemoryCache, new Mock<IEventPublisher>().Object);

            return new ThumbnailOptionSearchService(() => repositoryMock.Object, platformMemoryCache, crudService, Options.Create(new CrudOptions()));
        }
    }
}
