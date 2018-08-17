using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.ImageToolsModule.Core.Models;

namespace VirtoCommerce.ImageToolsModule.Data.BackwardsCompatibility
{
    /// <summary>
    /// Custom conveter to parse old option json 
    /// </summary>
    public class SettingJsonConverter : JsonConverter
    {
        #region Overrides of JsonConverter

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            var target = Create(jObject);
            return target;
        }

        private ThumbnailOption Create(JObject jObject)
        {
            ResizeMethod resultResizeMethod = default(ResizeMethod);
            var resizeMethod = jObject["method"];
            if (resizeMethod != null)
            {
                Enum.TryParse(resizeMethod.Value<string>(), out resultResizeMethod);
            }

            AnchorPosition resultAnchorPosition = default(AnchorPosition);
            var anchorPosition = jObject["anchorposition"];
            if (anchorPosition != null)
            {
                Enum.TryParse(anchorPosition.Value<string>(), out resultAnchorPosition);
            }

            JpegQuality resultJpegQuality = default(JpegQuality);
            var jpegQuality = jObject["jpegquality"];
            if (jpegQuality != null)
            {
                Enum.TryParse(jpegQuality.Value<string>(), out resultJpegQuality);
            }

            var result = new ThumbnailOption()
            {
                BackgroundColor = jObject["color"]?.Value<string>(),
                FileSuffix = jObject["alias"]?.Value<string>(),
                Width = jObject["width"]?.Value<int>(),
                Height = jObject["height"]?.Value<int>(),
                AnchorPosition = resultAnchorPosition,
                JpegQuality = resultJpegQuality,
                ResizeMethod = resultResizeMethod
            };

            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ThumbnailOption) == objectType;
        }

        #endregion
    }
}
