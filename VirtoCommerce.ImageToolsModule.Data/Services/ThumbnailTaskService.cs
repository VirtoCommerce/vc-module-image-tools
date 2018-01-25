namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    using VirtoCommerce.ImageToolsModule.Core.Models;
    using VirtoCommerce.ImageToolsModule.Core.Services;
    using VirtoCommerce.ImageToolsModule.Data.Repositories;

    public class ThumbnailTaskService : IThumbnailTaskService
    {
        private readonly IThumbnailRepository _repository;

        public ThumbnailTaskService(IThumbnailRepository repository)
        {
            this._repository = repository;
        }

        public void SaveChanges(ThumbnailTask[] options)
        {
            throw new System.NotImplementedException();
        }

        public ThumbnailTask[] GetByIds(string[] ids)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(string[] ids)
        {
            throw new System.NotImplementedException();
        }
    }
}