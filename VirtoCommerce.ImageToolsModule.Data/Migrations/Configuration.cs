namespace VirtoCommerce.ImageToolsModule.Data.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public sealed class Configuration : DbMigrationsConfiguration<VirtoCommerce.ImageToolsModule.Data.Repositories.ThumbnailRepositoryImpl>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations";
        }

        protected override void Seed(VirtoCommerce.ImageToolsModule.Data.Repositories.ThumbnailRepositoryImpl context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
