namespace VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration
{
    public interface IImagesChangesProvider
    {
        long GetTotalChangesCount();

        ImageChangeResult GetNextChangesBatch();
    }
}