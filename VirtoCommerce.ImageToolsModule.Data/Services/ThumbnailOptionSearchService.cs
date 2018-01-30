namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    using System.Linq;

    using VirtoCommerce.ImageToolsModule.Core.Models;
    using VirtoCommerce.ImageToolsModule.Core.Services;
    using VirtoCommerce.ImageToolsModule.Data.Repositories;

    public class ThumbnailOptionSearchService : IThumbnailOptionService
    {
        private readonly IThumbnailRepository _repository;

        public ThumbnailOptionSearchService()
        {
            
        }

        public ThumbnailOptionSearchService(IThumbnailRepository repository)
        {
            this._repository = repository;
        }

        public void SaveChanges(ThumbnailOption[] options)
        {

        }

        public ThumbnailOption[] GetByIds(string[] ids)
        {
            return _repository.GetThumbnailOptionsByIds(ids).Select(o =>
                {
                    var option = new ThumbnailOption();
                    return o.ToModel(option);
                }).ToArray();
        }

        public void Delete(string[] ids)
        {
            throw new System.NotImplementedException();
        }
    }
}