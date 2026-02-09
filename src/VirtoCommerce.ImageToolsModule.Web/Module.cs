using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.AssetsModule.Core.Events;
using VirtoCommerce.ImageToolsModule.Core;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.BackgroundJobs;
using VirtoCommerce.ImageToolsModule.Data.ExportImport;
using VirtoCommerce.ImageToolsModule.Data.Handlers;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.MySql;
using VirtoCommerce.ImageToolsModule.Data.PostgreSql;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;
using VirtoCommerce.ImageToolsModule.Data.SqlServer;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.Platform.Hangfire;

namespace VirtoCommerce.ImageToolsModule.Web
{
    public class Module : IModule, IExportSupport, IImportSupport, IHasConfiguration
    {
        private IApplicationBuilder _appBuilder;

        public ManifestModuleInfo ModuleInfo { get; set; }
        public IConfiguration Configuration { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<ThumbnailDbContext>(options =>
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
                var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

                switch (databaseProvider)
                {
                    case "MySql":
                        options.UseMySqlDatabase(connectionString);
                        break;
                    case "PostgreSql":
                        options.UsePostgreSqlDatabase(connectionString);
                        break;
                    default:
                        options.UseSqlServerDatabase(connectionString);
                        break;
                }
            });

            serviceCollection.AddTransient<IThumbnailRepository, ThumbnailRepository>();
            serviceCollection.AddTransient<Func<IThumbnailRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetService<IThumbnailRepository>());

            serviceCollection.AddTransient<IThumbnailOptionService, ThumbnailOptionService>();
            serviceCollection.AddTransient<IThumbnailOptionSearchService, ThumbnailOptionSearchService>();

            serviceCollection.AddTransient<IThumbnailTaskSearchService, ThumbnailTaskSearchService>();
            serviceCollection.AddTransient<IThumbnailTaskService, ThumbnailTaskService>();

            serviceCollection.AddTransient<IImageResizer, ImageResizer>();
            serviceCollection.AddTransient<IImageService, ImageService>();
            serviceCollection.AddTransient<IThumbnailGenerator, ThumbnailGenerator>();
            serviceCollection.AddTransient<IThumbnailGenerationProcessor, ThumbnailGenerationProcessor>();
            serviceCollection.AddTransient<IImagesChangesProvider, BlobImagesChangesProvider>();

            // SVG support
            serviceCollection.AddTransient<ISvgResizer, SvgResizer>();
            serviceCollection.AddTransient<ISvgService, SvgService>();

            // Image format validation
            serviceCollection.AddSingleton<IAllowedImageFormatsService, AllowedImageFormatsService>();

            // Format handlers (registered as collection for IThumbnailHandlerFactory)
            serviceCollection.AddTransient<IFormatThumbnailHandler, RasterThumbnailHandler>();
            serviceCollection.AddTransient<IFormatThumbnailHandler, SvgThumbnailHandler>();

            // Handler factory for format-based routing
            serviceCollection.AddSingleton<IThumbnailHandlerFactory, ThumbnailHandlerFactory>();

            serviceCollection.AddTransient<ThumbnailsExportImport>();
            serviceCollection.AddTransient<BlobCreatedEventHandler>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            _appBuilder = appBuilder;

            AbstractTypeFactory<ThumbnailOption>.RegisterType<ThumbnailOption>().MapToType<ThumbnailOptionEntity>();
            AbstractTypeFactory<ThumbnailTask>.RegisterType<ThumbnailTask>().MapToType<ThumbnailTaskEntity>();

            // Register event handlers
            appBuilder.RegisterEventHandler<BlobCreatedEvent, BlobCreatedEventHandler>();

            //Register module settings
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

            //Register module permissions
            var permissionsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "Thumbnail", ModuleConstants.Security.Permissions.AllPermissions);

            //Schedule periodic image processing job
            var recurringJobService = appBuilder.ApplicationServices.GetService<IRecurringJobService>();

            recurringJobService.WatchJobSetting(
                new SettingCronJobBuilder()
                    .SetEnablerSetting(ModuleConstants.Settings.General.EnableImageProcessJob)
                    .SetCronSetting(ModuleConstants.Settings.General.ImageProcessJobCronExpression)
                    .ToJob<ThumbnailProcessJob>(x => x.ProcessAll(JobCancellationToken.Null))
                    .Build());

            //Force migrations
            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");

                var thumbnailDbContext = serviceScope.ServiceProvider.GetRequiredService<ThumbnailDbContext>();
                if (databaseProvider == "SqlServer")
                {
                    thumbnailDbContext.Database.MigrateIfNotApplied(MigrationName.GetUpdateV2MigrationName(ModuleInfo.Id));
                }
                thumbnailDbContext.Database.Migrate();
            }
        }

        public void Uninstall()
        {
            // Nothing to do here
        }

        public async Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<ThumbnailsExportImport>().DoExportAsync(outStream, progressCallback, cancellationToken);
        }

        public async Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<ThumbnailsExportImport>().DoImportAsync(inputStream, progressCallback, cancellationToken);
        }
    }
}
