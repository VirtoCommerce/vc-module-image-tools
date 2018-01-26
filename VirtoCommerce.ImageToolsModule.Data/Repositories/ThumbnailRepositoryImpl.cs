namespace VirtoCommerce.ImageToolsModule.Data.Repositories
{
    using System.Data.Entity;
    using System.Linq;

    using VirtoCommerce.Platform.Data.Infrastructure;
    using VirtoCommerce.ImageToolsModule.Data.Models;
    using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

    /// <summary>
    /// The thumbnail repository impl.
    /// </summary>
    public class ThumbnailRepositoryImpl : EFRepositoryBase, IThumbnailRepository
    {
        public ThumbnailRepositoryImpl()
            : base()
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
            modelBuilder.Entity<ThumbnailTaskEntity>().HasMany(t => t.ThumbnailTaskOptions)
                .WithRequired(o => o.TaskEntity).WillCascadeOnDelete();

            modelBuilder.Entity<ThumbnailOptionEntity>().ToTable("ThumbnailOption").HasKey(t => t.Id).Property(t => t.Id);
        }

        public IQueryable<ThumbnailTaskEntity> ThumbnailTasks => GetAsQueryable<ThumbnailTaskEntity>();

        public IQueryable<ThumbnailOptionEntity> ThumbnailOptions => GetAsQueryable<ThumbnailOptionEntity>();

        public ThumbnailTaskEntity[] GetThumbnailTasksByIds(string[] ids)
        {
            return ThumbnailTasks.Include(t => t.ThumbnailTaskOptions).Where(t => ids.Contains(t.Id)).ToArray();
        }

        public ThumbnailOptionEntity[] GetThumbnailOptionsByIds(string[] ids)
        {
            return ThumbnailOptions.Where(o => ids.Contains(o.Id)).ToArray();
        }
    }
}