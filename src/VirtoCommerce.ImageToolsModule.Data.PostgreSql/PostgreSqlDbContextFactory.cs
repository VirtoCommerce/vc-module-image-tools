using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.ImageToolsModule.Data.Repositories;

namespace VirtoCommerce.ImageToolsModule.Data.PostgreSql
{
    public class PostgreSqlDbContextFactory : IDesignTimeDbContextFactory<ThumbnailDbContext>
    {
        public ThumbnailDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ThumbnailDbContext>();
            var connectionString = args.Any() ? args[0] : "User ID = postgres; Password = password; Host = localhost; Port = 5432; Database = virtocommerce3;";

            builder.UseNpgsql(
                connectionString,
                db => db.MigrationsAssembly(typeof(PostgreSqlDbContextFactory).Assembly.GetName().Name));

            return new ThumbnailDbContext(builder.Options);
        }
    }
}
