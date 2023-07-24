using System.Reflection;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.ImageToolsModule.Data.Models;

namespace VirtoCommerce.ImageToolsModule.Data.Repositories
{
    public class ThumbnailDbContext : DbContextWithTriggers
    {
        public ThumbnailDbContext(DbContextOptions<ThumbnailDbContext> options)
            : base(options)
        {
        }

        protected ThumbnailDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ThumbnailTaskEntity>().ToTable("ThumbnailTask").HasKey(t => t.Id);
            modelBuilder.Entity<ThumbnailTaskEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<ThumbnailTaskEntity>().Property(x => x.CreatedBy).HasMaxLength(64);
            modelBuilder.Entity<ThumbnailTaskEntity>().Property(x => x.ModifiedBy).HasMaxLength(64);

            modelBuilder.Entity<ThumbnailOptionEntity>().ToTable("ThumbnailOption").HasKey(t => t.Id);
            modelBuilder.Entity<ThumbnailOptionEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<ThumbnailOptionEntity>().Property(x => x.CreatedBy).HasMaxLength(64);
            modelBuilder.Entity<ThumbnailOptionEntity>().Property(x => x.ModifiedBy).HasMaxLength(64);

            modelBuilder.Entity<ThumbnailTaskOptionEntity>().ToTable("ThumbnailTaskOption").HasKey(x => x.Id);
            modelBuilder.Entity<ThumbnailTaskOptionEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();

            modelBuilder.Entity<ThumbnailTaskOptionEntity>()
                .HasOne(x => x.ThumbnailTask)
                .WithMany(t => t.ThumbnailTaskOptions)
                .HasForeignKey(x => x.ThumbnailTaskId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ThumbnailTaskOptionEntity>()
                .HasOne(x => x.ThumbnailOption)
                .WithMany()
                .HasForeignKey(x => x.ThumbnailOptionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Allows configuration for an entity type for different database types.
            // Applies configuration from all <see cref="IEntityTypeConfiguration{TEntity}" in VirtoCommerce.ImageToolsModule.Data.XXX project. /> 
            switch (Database.ProviderName)
            {
                case "Pomelo.EntityFrameworkCore.MySql":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.ImageToolsModule.Data.MySql"));
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.ImageToolsModule.Data.PostgreSql"));
                    break;
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.ImageToolsModule.Data.SqlServer"));
                    break;
            }
        }
    }
}
