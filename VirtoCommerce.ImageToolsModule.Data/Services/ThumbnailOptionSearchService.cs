namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    using VirtoCommerce.ImageToolsModule.Core.Models;
    using VirtoCommerce.ImageToolsModule.Core.Services;
    using VirtoCommerce.ImageToolsModule.Data.Repositories;

    public class ThumbnailOptionSearchService : IThumbnailOptionService
    {
        private readonly IThumbnailRepository _repository;

        public ThumbnailsOptionsController(IThumbnailRepository repository)
        {
            this._repository = repository;
        }

        public void SaveChanges(ThumbnailOption[] options)
        {
            throw new System.NotImplementedException();
        }

        public ThumbnailOption[] GetByIds(string[] ids)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(string[] ids)
        {
            throw new System.NotImplementedException();
        }
    }
}