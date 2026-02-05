namespace VirtoCommerce.ImageToolsModule.Core.Services
{
    /// <summary>
    /// Identifies the category of image format for processing routing
    /// </summary>
    public enum ImageFormatType
    {
        /// <summary>
        /// Raster/bitmap formats: JPEG, PNG, WebP, GIF, BMP, TIFF, etc.
        /// </summary>
        Raster,

        /// <summary>
        /// Vector formats: SVG, SVGZ, etc.
        /// </summary>
        Vector
    }
}
