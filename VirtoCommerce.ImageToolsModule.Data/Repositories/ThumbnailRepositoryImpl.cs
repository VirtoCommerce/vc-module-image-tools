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
                .HasRequired(x => x.ThumbnailTaskEntity)
                .WithMany(t => t.ThumbnailTaskOptionEntities)
                .HasForeignKey(x => x.ThumbnailTaskEntityId);

            modelBuilder.Entity<ThumbnailTaskOptionEntity>()
                .HasRequired(x => x.ThumbnailOptionEntity)
                .WithMany()
                .HasForeignKey(x=>x.ThumbnailOptionEntityId);
        }

        public IQueryable<ThumbnailTaskEntity> ThumbnailTaskEntities => GetAsQueryable<ThumbnailTaskEntity>();

        public IQueryable<ThumbnailOptionEntity> ThumbnailOptionsEntities => GetAsQueryable<ThumbnailOptionEntity>();

        public IQueryable<ThumbnailTaskOptionEntity> ThumbnailTaskOptionEntities => GetAsQueryable<ThumbnailTaskOptionEntity>();

        public ThumbnailTaskEntity[] GetThumbnailTasksByIds(string[] ids)
        {
            return ThumbnailTaskEntities.Include(t => t.ThumbnailTaskOptionEntities).Where(t => ids.Contains(t.Id)).ToArray();
        }

        public ThumbnailOptionEntity[] GetThumbnailOptionsByIds(string[] ids)
        {
            return ThumbnailOptionsEntities.Where(o => ids.Contains(o.Id)).ToArray();
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