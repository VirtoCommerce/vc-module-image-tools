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
        public void SerchTasks_ThumbnailOptionSearchCriteria_ReturnsGenericSearchResponseOfTasksInExpectedOrder()
        {
            var optionEntities = ThumbnailOptionEntitesDataSource.ToArray();
            var expectedOptions = ThumbnailOptionDataSource.OrderBy(t => t.Name).ThenByDescending(t => t.Width).ToArray();

            var criteria = new ThumbnailOptionSearchCriteria { Sort = "Name:asc;Width:desc" };

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailOptionsByIds(It.IsIn<string[]>()))
                .Returns((string[] ids) => { return optionEntities.Where(t => ids.Contains(t.Id)).ToArray(); });

            var sut = new ThumbnailOptionSearchService(() => mock.Object);

            var resultTasks = sut.Search(criteria);

            Assert.Equal(expectedOptions, resultTasks.Results);
        }

        private static IEnumerable<ThumbnailOptionEntity> ThumbnailOptionEntitesDataSource
        {
            get
            {
                int i = 0;
                yield return new ThumbnailOptionEntity() { Id = $"Option {++i}" };
                yield return new ThumbnailOptionEntity() { Id = $"Option {++i}" };
                yield return new ThumbnailOptionEntity() { Id = $"Option {++i}" };
            }
        }

        private static IEnumerable<ThumbnailOption> ThumbnailOptionDataSource
        {
            get
            {
                int i = 0;
                yield return new ThumbnailOption() { Id = $"Option {++i}", Name = "New Name" };
                yield return new ThumbnailOption() { Id = $"Option {++i}", Name = "New Name" };
                yield return new ThumbnailOption() { Id = $"Option {++i}", Name = "New Name" };
            }
        }
    }
}