using System;
using System.IO;
using Hangfire;
using Microsoft.Practices.Unity;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Web.BackgroundJobs;
using VirtoCommerce.ImageToolsModule.Web.ExportImport;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.ImageToolsModule.Web
{
    /// <summary>
    /// Module
    /// </summary>
    public class Module : ModuleBase, ISupportExportImportModule
    {
        private readonly IUnityContainer _container;
        private static readonly string _connectionString = ConfigurationHelper.GetNonEmptyConnectionStringValue("VirtoCommerce");

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container"></param>
        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase()
        {
            base.SetupDatabase();

            using (var db = new ThumbnailRepositoryImpl(_connectionString, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<ThumbnailRepositoryImpl, Data.Migrations.Configuration>();

                initializer.InitializeDatabase(db);
            }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            _container.RegisterType<IThumbnailRepository>(new InjectionFactory(c => new ThumbnailRepositoryImpl(_connectionString, _container.Resolve<AuditableInterceptor>(), new EntityPrimaryKeyGeneratorInterceptor())));

            _container.RegisterType<IThumbnailOptionService, ThumbnailOptionService>();
            _container.RegisterType<IThumbnailOptionSearchService, ThumbnailOptionSearchService>();

            _container.RegisterType<IThumbnailTaskService, ThumbnailTaskService>();
            _container.RegisterType<IThumbnailTaskSearchService, ThumbnailTaskSearchService>();

            _container.RegisterType<IImageResizer, ImageResizer>();
            _container.RegisterType<IImageService, ImageService>();
            _container.RegisterType<IThumbnailGenerator, DefaultThumbnailGenerator>();
            _container.RegisterType<IThumbnailGenerationProcessor, ThumbnailGenerationProcessor>();
            _container.RegisterType<IImagesChangesProvider, BlobImagesChangesProvider>();

#pragma warning disable 612, 618
            _container.RegisterType<IThumbnailService, ThumbnailService>();
#pragma warning restore 612, 618
        }

        public override void PostInitialize()
        {
            base.PostInitialize();

            var settingsManager = _container.Resolve<ISettingsManager>();

            //Schedule periodic image processing job
            var processJobEnabled = settingsManager.GetValue("ImageTools.Thumbnails.EnableImageProcessjob", false);
            if (processJobEnabled)
            {
                var cronExpression = settingsManager.GetValue("ImageTools.Thumbnails.ImageProcessjobCronExpression", "0 0 * * *");
                RecurringJob.AddOrUpdate<ThumbnailProcessJob>("ProcessAllImageTasksJob", x => x.ProcessAll(JobCancellationToken.Null), cronExpression);
            }
            else
            {
                RecurringJob.RemoveIfExists("ProcessAllImageTasksJob");
            }

        }
        #endregion


        #region ISupportExportImportModule Members

        public void DoExport(Stream outStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<ThumbnailsExportImport>();
            exportJob.DoExport(outStream, manifest, progressCallback);
        }

        public void DoImport(Stream inputStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var exportJob = _container.Resolve<ThumbnailsExportImport>();
            exportJob.DoImport(inputStream, manifest, progressCallback);
        }

        public string ExportDescription
        {
            get
            {
                var settingManager = _container.Resolve<ISettingsManager>();
                return settingManager.GetValue("ImageTools.ExportImport.Description", string.Empty);
            }
        }

        #endregion
    }
}