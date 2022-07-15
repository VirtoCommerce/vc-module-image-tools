using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.ImageTools.ImageAbstractions;
using VirtoCommerce.ImageToolsModule.Core;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.ExportImport;
using VirtoCommerce.ImageToolsModule.Data.Models;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Web.BackgroundJobs;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.Platform.Hangfire;
using VirtoCommerce.Platform.Hangfire.Extensions;

namespace VirtoCommerce.ImageToolsModule.Web
{
    public class Module : IModule, IExportSupport, IImportSupport
    {
        public ManifestModuleInfo ModuleInfo { get; set; }
        private IApplicationBuilder _appBuilder;
        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<ThumbnailDbContext>((provider, options) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                options.UseSqlServer(configuration.GetConnectionString(ModuleInfo.Id) ?? configuration.GetConnectionString("VirtoCommerce"));
            });

            serviceCollection.AddTransient<IThumbnailRepository, ThumbnailRepository>();
            serviceCollection.AddTransient<Func<IThumbnailRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetService<IThumbnailRepository>());

            serviceCollection.AddTransient<IThumbnailOptionService, ThumbnailOptionService>();
            serviceCollection.AddTransient<IThumbnailOptionSearchService, ThumbnailOptionSearchService>();

            serviceCollection.AddTransient<IThumbnailTaskSearchService, ThumbnailTaskSearchService>();
            serviceCollection.AddTransient<IThumbnailTaskService, ThumbnailTaskService>();

            serviceCollection.AddTransient<IImageResizer, ImageResizer>();
            serviceCollection.AddTransient<IImageService, ImageService>();
            serviceCollection.AddTransient<IThumbnailGenerator, DefaultThumbnailGenerator>();
            serviceCollection.AddTransient<IThumbnailGenerationProcessor, ThumbnailGenerationProcessor>();
            serviceCollection.AddTransient<IImagesChangesProvider, BlobImagesChangesProvider>();
            serviceCollection.AddTransient<ThumbnailsExportImport>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            _appBuilder = appBuilder;

            AbstractTypeFactory<ThumbnailOption>.RegisterType<ThumbnailOption>().MapToType<ThumbnailOptionEntity>();
            AbstractTypeFactory<ThumbnailTask>.RegisterType<ThumbnailTask>().MapToType<ThumbnailTaskEntity>();

            //Register module settings
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

            //Register module permissions
            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x => new Permission() { GroupName = "Thumbnail", Name = x }).ToArray());



            //Schedule periodic image processing job
            var recurringJobManager = appBuilder.ApplicationServices.GetService<IRecurringJobManager>();
            var settingsManager = appBuilder.ApplicationServices.GetService<ISettingsManager>();

            recurringJobManager.WatchJobSetting(
                settingsManager,
                new SettingCronJobBuilder()
                    .SetEnablerSetting(ModuleConstants.Settings.General.EnableImageProcessJob)
                    .SetCronSetting(ModuleConstants.Settings.General.ImageProcessJobCronExpression)
                    .ToJob<ThumbnailProcessJob>(x => x.ProcessAll(JobCancellationToken.Null))
                    .Build());

            //Force migrations
            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var thumbnailDbContext = serviceScope.ServiceProvider.GetRequiredService<ThumbnailDbContext>();
                thumbnailDbContext.Database.MigrateIfNotApplied(MigrationName.GetUpdateV2MigrationName(ModuleInfo.Id));
                thumbnailDbContext.Database.EnsureCreated();
                thumbnailDbContext.Database.Migrate();
            }
        }

        public void Uninstall()
        {
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
