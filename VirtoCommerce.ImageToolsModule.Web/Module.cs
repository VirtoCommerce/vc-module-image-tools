using Microsoft.Practices.Unity;
using VirtoCommerce.Platform.Core.Modularity;

namespace VirtoCommerce.ImageToolsModule.Web
{
    /// <summary>
    /// Module
    /// </summary>
    public class Module : ModuleBase
    {
        private readonly IUnityContainer _container;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container"></param>
        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        /// <summary>
        /// Initialization
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            //_container.RegisterType<IThumbnailService, ThumbnailService>();
            //_container.RegisterType<IImageResizer, ImageResizer>();
        }

        #endregion
    }
}