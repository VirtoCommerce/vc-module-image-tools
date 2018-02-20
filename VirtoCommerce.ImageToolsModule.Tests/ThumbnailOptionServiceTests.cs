using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class ThumbnailOptionServiceTests
    {
        [Fact]
        public void GetByIds_ArrayOfIdis_ReturnsArrayOfThumbnailOption()
        {
            var optionEntites = ThumbnailOptionEntitesDataSource.ToArray();

            var ids = optionEntites.Select(t => t.Id).ToArray();
            var tasks = optionEntites.Select(t => t.ToModel(new ThumbnailOption())).ToArray();

            var mock = new Mock<IThumbnailRepository>();
            mock.Setup(r => r.GetThumbnailOptionsByIds(It.IsIn<string[]>(ids)))
                .Returns(optionEntites.Where(o => ids.Contains(o.Id))
                .ToArray());

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
            mock.SetupGet(x => x.UnitOfWork).Returns(new Mock<IUnitOfWork>().Object);
            mock.Setup(r => r.RemoveThumbnailOptionsByIds(It.IsIn<string[]>(ids)))
                .Callback((string[] arr) =>
                {
                    var entities = optionEntites.Where(e => arr.Contains(e.Id)).ToList();
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
            mock.SetupGet(x => x.UnitOfWork).Returns(new Mock<IUnitOfWork>().Object);
            mock.Setup(r => r.GetThumbnailOptionsByIds(It.IsAny<string[]>()))
                .Returns((string[] ids) =>
                {
                    var result = optionEntities.Where(t => ids.Contains(t.Id)).ToArray();
                    return result;
                });

            var sut = new ThumbnailOptionService(() => mock.Object);
            sut.SaveOrUpdate(options);

            Assert.Contains(optionEntities, o => o.Name == "New Name");
        }

        [Fact]
        public void SaveChanges_ArrayOfThumbnailOptions_NewThumbnailOptionsSaved()
        {
            var optionEntities = ThumbnailOptionEntitesDataSource.ToList();

            var mock = new Mock<IThumbnailRepository>();
            mock.SetupGet(x => x.UnitOfWork).Returns(new Mock<IUnitOfWork>().Object);
            mock.Setup(x => x.Add(It.IsAny<ThumbnailOptionEntity>()))
                .Callback((ThumbnailOptionEntity entity) =>
                {
                    optionEntities.Add(entity);
                });
            mock.Setup(r => r.GetThumbnailOptionsByIds(It.IsAny<string[]>()))
                .Returns((string[] ids) =>
                {
                    return optionEntities.Where(t => ids.Contains(t.Id)).ToArray();
                });

            var sut = new ThumbnailOptionService(() => mock.Object);
            sut.SaveOrUpdate(new[]
            {
                new ThumbnailOption()
                {
                    Id = "NewOptionId", Name = "New Option name"
                }
            });

            Assert.Contains(optionEntities, x => x.Id == "NewOptionId");
        }

        private static IEnumerable<ThumbnailOptionEntity> ThumbnailOptionEntitesDataSource
        {
            get
            {
                yield return new ThumbnailOptionEntity() { Id = "Option 1" };
                yield return new ThumbnailOptionEntity() { Id = "Option 2" };
                yield return new ThumbnailOptionEntity() { Id = "Option 3" };
            }
        }

        private static IEnumerable<ThumbnailOption> ThumbnailOptionDataSource
        {
            get
            {
                yield return new ThumbnailOption() { Id = "Option 1", Name = "New Name" };
                yield return new ThumbnailOption() { Id = "Option 2", Name = "New Name" };
                yield return new ThumbnailOption() { Id = "Option 3", Name = "New Name" };
            }
        }
    }
}
