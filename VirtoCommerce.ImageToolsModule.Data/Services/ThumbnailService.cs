using System;
using System.Threading.Tasks;

namespace VirtoCommerce.ImageToolsModule.Data.Services
{
    /// <summary>
    /// 
    /// </summary>
    [Obsolete]
    public class ThumbnailService : IThumbnailService
    {
        #region Implementation of IThumbnailService

        public Task<bool> GenerateAsync(string imageUrl, bool isRegenerateAll)
        {
            throw new NotImplementedException();
        }

        public string[] GetThumbnails(string imageUrl, string[] aliases)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}