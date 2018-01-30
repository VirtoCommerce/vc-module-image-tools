using Moq;
using NUnit.Framework;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    [TestFixture]
    public class ThumbnailTaskSearchServiceTests
    {
        private IThumbnailTaskSearchService _searchService;
        
        [SetUp]
        private void Init()
        {
            _searchService = new ThumbnailTaskSearchService();
        }
        
        [Test]
        public void SerchTasks_ThumbnailOptionSearchCriteria_ReturnsGenericSearchResponseOfTasksObject()
        {
            var criteria = new ThumbnailOptionSearchCriteria();
            
            var result = _searchService.SerchTasks(criteria);
        }
    }
}