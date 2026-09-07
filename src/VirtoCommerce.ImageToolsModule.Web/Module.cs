using System;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
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
using VirtoCommerce.ImageToolsModule.Data.Jobs;
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
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.Platform.Data.MySql.Extensions;
using VirtoCommerce.Platform.Data.PostgreSql.Extensions;
using VirtoCommerce.Platform.Data.SqlServer.Extensions;

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
                        options.UseMySqlDatabase(connectionString, typeof(MySqlDataAssemblyMarker), Configuration);
                        break;
                    case "PostgreSql":
                        options.UsePostgreSqlDatabase(connectionString, typeof(PostgreSqlDataAssemblyMarker), Configuration);
                        break;
                    default:
                        options.UseSqlServerDatabase(connectionString, typeof(SqlServerDataAssemblyMarker), Configuration);
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

            serviceCollection.AddTransient<ThumbnailProcessJob>();
            serviceCollection.AddBackgroundJob<ThumbnailProcessJobHandler, ThumbnailProcessJobPayload>();

            // Schedule periodic image processing. Registered here rather than in PostInitialize: the schedule is now a
            // DI registration the engine module picks up, not an imperative call on a resolved service.
            serviceCollection.AddRecurringJob<ThumbnailProcessAllJobHandler, ThumbnailProcessAllJobPayload>(schedule => schedule
                .WithId(nameof(ThumbnailProcessAllJobHandler))
                // Carries over [AutomaticRetry(Attempts = 0)] from the Hangfire job: a failed scheduled run is
                // superseded by the next occurrence, so retrying it only duplicates a long generation pass.
                .WithMaxRetryAttempts(0)
                .FromSettings(
                    ModuleConstants.Settings.General.EnableImageProcessJob,
                    ModuleConstants.Settings.General.ImageProcessJobCronExpression));
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

        public async Task ExportAsync(Stream outStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<ThumbnailsExportImport>().DoExportAsync(outStream, progressCallback, cancellationToken);
        }

        public async Task ImportAsync(Stream inputStream, ExportImportOptions options, Action<ExportImportProgressInfo> progressCallback, CancellationToken cancellationToken)
        {
            await _appBuilder.ApplicationServices.GetRequiredService<ThumbnailsExportImport>().DoImportAsync(inputStream, progressCallback, cancellationToken);
        }
    }
}
