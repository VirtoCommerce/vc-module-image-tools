namespace VirtoCommerce.ImageToolsModule.Data.Repositories
{
    using System.Data.Entity;
    using System.Linq;
    using VirtoCommerce.Platform.Data.Infrastructure;
    using VirtoCommerce.ImageToolsModule.Data.Models;

    /// <summary>
    /// The thumbnail repository impl.
    /// </summary>
    public class ThumbnailRepositoryImpl : EFRepositoryBase, IThumbnailRepository
    {
        public ThumbnailRepositoryImpl()
        {
            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            NewMethod(modelBuilder);

        }

        private static void NewMethod(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ThumbnailTaskEntity>().ToTable("ThumbnailTask");
            modelBuilder.Entity<ThumbnailTaskEntity>().HasKey(t => t.Id).Property(t => t.Id);
            modelBuilder.Entity<ThumbnailTaskEntity>().HasMany(t => t.ThumbnailTaskOptions)
                 .WithRequired(o => o.TaskEntity).WillCascadeOnDelete();

            modelBuilder.Entity<ThumbnailOptionEntity>().ToTable("ThumbnailOption");
            modelBuilder.Entity<ThumbnailOptionEntity>().HasKey(t => t.Id).Property(t => t.Id);
        }

        public IQueryable<ThumbnailTaskEntity> ThumbnailTasks
        {
            get { return GetAsQueryable<ThumbnailTaskEntity>(); }
        }

        public IQueryable<ThumbnailOptionEntity> ThumbnailOptions
        {
            get { return GetAsQueryable<ThumbnailOptionEntity>(); }
        } 

        public ThumbnailTaskEntity[] GetThumbnailTasksByIds(string[] ids)
        {
            return ThumbnailTasks.Where(t => ids.Contains(t.Id)).ToArray();
        }

        public ThumbnailOptionEntity[] GetThumbnailOptionsByIds(string[] ids)
        {
            return ThumbnailOptions.Where(o => ids.Contains(o.Id)).ToArray();
        }
    }
}