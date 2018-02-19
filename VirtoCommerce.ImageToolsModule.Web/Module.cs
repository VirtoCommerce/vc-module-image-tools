using System;
using Microsoft.Practices.Unity;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.Repositories;
using VirtoCommerce.ImageToolsModule.Data.Services;
using VirtoCommerce.ImageToolsModule.Data.ThumbnailGeneration;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.ImageToolsModule.Web
{
    /// <summary>
    /// Module
    /// </summary>
    public class Module : ModuleBase
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
            _container.RegisterType<IThumbnailGenerator, DefaultThumbnailGenerator>();
            _container.RegisterType<IThumbnailGenerationProcessor, ThumbnailGenerationProcessor>();
            _container.RegisterType<IImagesChangesProvider, BlobImagesChangesProvider>();

            _container.RegisterType<IThumbnailService, ThumbnailService>();
        }

        #endregion
    }
}