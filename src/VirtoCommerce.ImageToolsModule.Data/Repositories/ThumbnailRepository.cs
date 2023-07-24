using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.ImageToolsModule.Data.Repositories
{
    public class ThumbnailRepository : DbContextRepositoryBase<ThumbnailDbContext>, IThumbnailRepository
    {
        public ThumbnailRepository(ThumbnailDbContext dbContext)
            : base(dbContext)
        {
        }

        public IQueryable<ThumbnailTaskEntity> ThumbnailTasks => DbContext.Set<ThumbnailTaskEntity>();

        public IQueryable<ThumbnailOptionEntity> ThumbnailOptions => DbContext.Set<ThumbnailOptionEntity>();

        public async Task<IList<ThumbnailTaskEntity>> GetThumbnailTasksByIdsAsync(IList<string> ids)
        {
            return await ThumbnailTasks
                .Include(t => t.ThumbnailTaskOptions)
                .ThenInclude(o => o.ThumbnailOption)
                .Where(t => ids.Contains(t.Id))
                .ToListAsync();
        }

        public async Task<IList<ThumbnailOptionEntity>> GetThumbnailOptionsByIdsAsync(IList<string> ids)
        {
            return await ThumbnailOptions
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();
        }
    }
}
