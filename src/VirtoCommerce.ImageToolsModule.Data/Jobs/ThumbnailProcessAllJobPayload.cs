namespace VirtoCommerce.ImageToolsModule.Data.Jobs
{
    /// <summary>
    /// Payload of the recurring job that runs every configured thumbnail task. Carries no data: the schedule fires
    /// with no arguments and the job discovers the tasks itself. It exists because the job API is payload-addressed,
    /// and it stays a class so a partner module can extend it via AbstractTypeFactory.
    /// </summary>
    public class ThumbnailProcessAllJobPayload
    {
    }
}
