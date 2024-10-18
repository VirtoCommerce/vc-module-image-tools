using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ImageToolsModule.Core
{
    [ExcludeFromCodeCoverage]
    public static class ModuleConstants
    {
        public static class Security
        {
            public static class Permissions
            {
                public const string Access = "thumbnail:access",
                    Create = "thumbnail:create",
                    Delete = "thumbnail:delete",
                    Update = "thumbnail:update",
                    Read = "thumbnail:read";

                public static string[] AllPermissions { get; } = { Access, Create, Delete, Update, Read };
            }
        }

        public static class Settings
        {
            public static class General
            {
                public static SettingDescriptor EnableImageProcessJob { get; } = new()
                {
                    Name = "ImageTools.Thumbnails.EnableImageProcessJob",
                    GroupName = "Thumbnail|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false,
                };

                public static SettingDescriptor EventBasedThumbnailGeneration { get; } = new()
                {
                    Name = "ImageTools.Thumbnails.EventBasedThumbnailGeneration",
                    GroupName = "Thumbnail|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true,
                };

                public static SettingDescriptor ImageProcessJobCronExpression { get; } = new()
                {
                    Name = "ImageTools.Thumbnails.ImageProcessJobCronExpression",
                    GroupName = "Thumbnail|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "0 0 * * *",
                };

                public static SettingDescriptor ProcessBatchSize { get; } = new()
                {
                    Name = "ImageTools.Thumbnails.ProcessBatchSize",
                    GroupName = "Thumbnail|General",
                    ValueType = SettingValueType.Integer,
                    DefaultValue = 50,
                };

                public static SettingDescriptor AllowedImageFormats { get; } = new()
                {
                    Name = "ImageTools.Thumbnails.AllowedImageFormats",
                    GroupName = "Thumbnail|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = string.Empty,
                    IsDictionary = true,
                    AllowedValues = [
                        JpegFormat.Instance.Name,
                        PngFormat.Instance.Name,
                        WebpFormat.Instance.Name]
                };

                public static IEnumerable<SettingDescriptor> AllGeneralSettings
                {
                    get
                    {
                        yield return EnableImageProcessJob;
                        yield return EventBasedThumbnailGeneration;
                        yield return ImageProcessJobCronExpression;
                        yield return ProcessBatchSize;
                        yield return AllowedImageFormats;
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> AllSettings => General.AllGeneralSettings;
        }
    }
}
