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
    class ThumbnailOptionServiceTests
    {
        [Fact]
        public void GetByIds_ArrayOfIdis_ReturnsArrayOfThumbnailOption()
        {
            var optionEntites = ThumbnailOptionEntitesDataSource.ToArray();

            var ids = optionEntites.Select(t => t.Id).ToArray();
            var tasks = optionEntites.Select(t => t.ToModel(new ThumbnailOption())).ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailOptionsByIds(It.IsIn<string[]>(ids))).Returns(optionEntites.Where(o => ids.Contains(o.Id)).ToArray());

            var sut = new ThumbnailOptionService(() => mock.Object);
            var result = sut.GetByIds(ids);

            Assert.Equal(result.Length, tasks.Length);
        }

        [Fact]
        public void Delete_ThumbnailOptionIds_DeletedThumbnailOptionWithPassedIds()
        {
            var optionEntites = ThumbnailOptionEntitesDataSource.ToList();

            var ids = optionEntites.Select(t => t.Id).ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.RemoveThumbnailOptionsByIds(It.IsIn<string[]>(ids)))
                .Callback((string[] arr) =>
                {
                    var entities = optionEntites.Where(e => arr.Contains(e.Id));
                    foreach (var entity in entities)
                    {
                        optionEntites.Remove(entity);
                    }
                });

            var sut = new ThumbnailOptionService(() => mock.Object);
            sut.RemoveByIds(ids);

            Assert.Empty(optionEntites);
        }

        [Fact]
        public void SaveChanges_ArrayOfThumbnailOptions_ThumbnailOptionsUpdated()
        {
            var optionEntities = ThumbnailOptionEntitesDataSource.ToArray();
            var options = ThumbnailOptionDataSource.ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailOptionsByIds(It.IsIn<string[]>()))
                .Returns((string[] ids) => { return optionEntities.Where(t => ids.Contains(t.Id)).ToArray(); });

            var sut = new ThumbnailOptionService(() => mock.Object);
            sut.SaveOrUpdate(options);

            Assert.Contains(optionEntities, o => o.Name == "New Name");
        }

        [Fact]
        public void SaveChanges_ArrayOfThumbnailOptions_NewThumbnailOptionsSaved()
        {
            var options = ThumbnailOptionDataSource.ToArray();
            var optionEntities = new List<ThumbnailOptionEntity>();

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailOptionsByIds(It.IsIn<string[]>()))
                .Returns((string[] ids) => { return optionEntities.Where(t => ids.Contains(t.Id)).ToArray(); });

            var sut = new ThumbnailOptionService(() => mock.Object);
            sut.SaveOrUpdate(options);

            Assert.NotEmpty(optionEntities);
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
