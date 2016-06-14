using Microsoft.Practices.Unity;
using VirtoCommerce.ImageToolsModule.Web.Services;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.ImageToolsModule.Web
{
    public class Module : ModuleBase
    {
        private const string _connectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members


        public override void Initialize()
        {
            base.Initialize();
            _container.RegisterType<IThumbnailService, ThumbnailService>();
            _container.RegisterType<IImageResize, ImageResize>();

        }

        #endregion

    }
}
