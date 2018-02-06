using System.Data.Entity;
using System.Linq;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.ImageToolsModule.Data.Repositories
{
    /// <summary>
    /// The thumbnail repository implementation.
    /// </summary>
    public class ThumbnailRepositoryImpl : EFRepositoryBase, IThumbnailRepository
    {
        public ThumbnailRepositoryImpl() : base()
        {
        }

        public ThumbnailRepositoryImpl(string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, null, interceptors)
        {
            Database.SetInitializer<ThumbnailRepositoryImpl>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ThumbnailTaskEntity>().ToTable("ThumbnailTask").HasKey(t => t.Id).Property(t => t.Id);
            modelBuilder.Entity<ThumbnailOptionEntity>().ToTable("ThumbnailOption").HasKey(t => t.Id).Property(t => t.Id);
            modelBuilder.Entity<ThumbnailTaskOptionEntity>().ToTable("ThumbnailTaskOption").HasKey(x => x.Id);

            modelBuilder.Entity<ThumbnailTaskOptionEntity>()
                .HasRequired(x => x.ThumbnailTask)
                .WithMany(t => t.ThumbnailTaskOptions)
                .HasForeignKey(x => x.ThumbnailTaskId);

            modelBuilder.Entity<ThumbnailTaskOptionEntity>()
                .HasRequired(x => x.ThumbnailOption)
                .WithMany()
                .HasForeignKey(x=>x.ThumbnailOptionId);
        }

        public IQueryable<ThumbnailTaskEntity> ThumbnailTasks => GetAsQueryable<ThumbnailTaskEntity>();

        public IQueryable<ThumbnailOptionEntity> ThumbnailOptions => GetAsQueryable<ThumbnailOptionEntity>();

        public IQueryable<ThumbnailTaskOptionEntity> ThumbnailTaskOption => GetAsQueryable<ThumbnailTaskOptionEntity>();

        public ThumbnailTaskEntity[] GetThumbnailTasksByIds(string[] ids)
        {
            return ThumbnailTasks.Include(t => t.ThumbnailTaskOptions).Where(t => ids.Contains(t.Id)).ToArray();
        }

        public ThumbnailOptionEntity[] GetThumbnailOptionsByIds(string[] ids)
        {
            return ThumbnailOptions.Where(o => ids.Contains(o.Id)).ToArray();
        }

        public void RemoveThumbnailTasksByIds(string[] ids)
        {
            foreach (var taskEntity in GetThumbnailTasksByIds(ids))
            {
                Remove(taskEntity);
            }
        }

        public void RemoveThumbnailOptionsByIds(string[] ids)
        {
            foreach (var optionEntity in GetThumbnailOptionsByIds(ids))
            {
                Remove(optionEntity);
            }
        }
    }
}