using System;
using System.Data.Entity;
using System.Linq;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.ImageToolsModule.Data.Repositories
{
    /// <summary>
    /// The thumbnail repository impl.
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
            #region ThumbnailTaskEntity

            modelBuilder.Entity<ThumbnailTaskEntity>().ToTable("ThumbnailTask").HasKey(t => t.Id).Property(t => t.Id);
            modelBuilder.Entity<ThumbnailTaskEntity>().HasMany(t => t.ThumbnailTaskOptions)
                .WithRequired(x => x.TaskEntity).WillCascadeOnDelete();

            #endregion

            #region ThumbnailOptionEntity

            modelBuilder.Entity<ThumbnailOptionEntity>().ToTable("ThumbnailOption").HasKey(t => t.Id).Property(t => t.Id);
            modelBuilder.Entity<ThumbnailOptionEntity>().HasMany(o => o.ThumbnailTaskOptions)
                .WithRequired(x => x.OptionEntity).WillCascadeOnDelete();

            #endregion

            #region ThumbnailTaskOptionEntity

            modelBuilder.Entity<ThumbnailTaskOptionEntity>().ToTable("ThumbnailTaskOption").HasKey(x => x.Id);
            modelBuilder.Entity<ThumbnailTaskOptionEntity>().HasRequired(x => x.TaskEntity)
                .WithMany(t => t.ThumbnailTaskOptions).WillCascadeOnDelete();
            modelBuilder.Entity<ThumbnailTaskOptionEntity>().HasRequired(x => x.OptionEntity)
                .WithMany(o => o.ThumbnailTaskOptions).WillCascadeOnDelete();

            #endregion
        }

        public IQueryable<ThumbnailTaskEntity> ThumbnailTaskEntities => GetAsQueryable<ThumbnailTaskEntity>();

        public IQueryable<ThumbnailOptionEntity> ThumbnailOptionsEntities => GetAsQueryable<ThumbnailOptionEntity>();
        
        public IQueryable<ThumbnailTaskOptionEntity> ThumbnailTaskOptionEntities => GetAsQueryable<ThumbnailTaskOptionEntity>();
        

        public ThumbnailTaskEntity[] GetThumbnailTasksByIds(string[] ids)
        {
            return ThumbnailTaskEntities.Include(t => t.ThumbnailTaskOptions).Where(t => ids.Contains(t.Id)).ToArray();
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