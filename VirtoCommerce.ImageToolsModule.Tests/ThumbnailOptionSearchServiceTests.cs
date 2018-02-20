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
    public class ThumbnailOptionSearchServiceTests
    {
        [Fact]
        public void Search_ThumbnailOptionSearchCriteria_ReturnsGenericSearchResponseOfTasksInExpectedOrder()
        {
            var repoMock = GetOptionsRepositoryMock();
            var target = new ThumbnailOptionSearchService(() => repoMock.Object);
            var criteria = new ThumbnailOptionSearchCriteria() { Sort = "Name:desc;FileSuffix:desc" };
            var resultTasks = target.Search(criteria);

            var expectedTasks = ThumbnailTaskEntitysDataSource.Select(x => x.ToModel(new ThumbnailOption())).OrderByDescending(t => t.Name).ThenByDescending(t => t.FileSuffix).ToArray();
            Assert.Equal(expectedTasks, resultTasks.Results);
        }

        public Mock<IThumbnailRepository> GetOptionsRepositoryMock()
        {
            var entites = ThumbnailTaskEntitysDataSource.ToList();
            var entitesQuerableMock = TestUtils.CreateQuerableMock(entites);
            var repoMock = new Mock<IThumbnailRepository>();

            repoMock.Setup(x => x.ThumbnailOptions).Returns(entitesQuerableMock.Object);

            repoMock.Setup(x => x.GetThumbnailOptionsByIds(It.IsAny<string[]>()))
                .Returns((string[] ids) =>
                {
                    return entitesQuerableMock.Object.Where(t => ids.Contains(t.Id)).ToArray();
                });

            return repoMock;
        }

        public IEnumerable<ThumbnailOptionEntity> ThumbnailTaskEntitysDataSource
        {
            get
            {
                yield return new ThumbnailOptionEntity() { Id = "Option1", Name = "Name 1", FileSuffix = "SuffixName4" };
                yield return new ThumbnailOptionEntity() { Id = "Option2", Name = "NameLong 2", FileSuffix = "SuffixName3" };
                yield return new ThumbnailOptionEntity() { Id = "Option3", Name = "Name 3", FileSuffix = "SuffixName2" };
                yield return new ThumbnailOptionEntity() { Id = "Option4", Name = "NameLong 4", FileSuffix = "SuffixName1" };
            }
        }
    }
}