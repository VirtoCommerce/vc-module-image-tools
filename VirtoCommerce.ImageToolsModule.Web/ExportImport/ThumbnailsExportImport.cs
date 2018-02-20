using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.ImageToolsModule.Web.ExportImport
{
    public class ThumbnailsExportImport
    {
        private readonly IThumbnailTaskSearchService _taskSearchService;
        private readonly IThumbnailOptionSearchService _optionSearchService;
        private readonly IThumbnailTaskService _taskService;
        private readonly IThumbnailOptionService _optionService;

        private const int _batchSize = 50;
        private readonly JsonSerializer _serializer;

        public ThumbnailsExportImport(IThumbnailTaskSearchService taskSearchService,
            IThumbnailOptionSearchService optionSearchService, IThumbnailTaskService taskService,
            IThumbnailOptionService optionService)
        {
            _taskSearchService = taskSearchService;
            _optionSearchService = optionSearchService;
            _taskService = taskService;
            _optionService = optionService;

            _serializer = new JsonSerializer();
            _serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            _serializer.Formatting = Formatting.Indented;
            _serializer.NullValueHandling = NullValueHandling.Ignore;
        }

        public void DoExport(Stream outStream, PlatformExportManifest manifest,
            Action<ExportImportProgressInfo> progressCallback)
        {
            var progressInfo = new ExportImportProgressInfo {Description = "loading data..."};
            progressCallback(progressInfo);

            using (StreamWriter sw = new StreamWriter(outStream, Encoding.UTF8))
            {
                using (JsonTextWriter writer = new JsonTextWriter(sw))
                {
                    writer.WriteStartObject();

                    ExportOptions(writer, _serializer, manifest, progressInfo, progressCallback);
                    ExportTasks(writer, _serializer, manifest, progressInfo, progressCallback);

                    writer.WriteEndObject();
                    writer.Flush();
                }
            }
        }

        private void ExportOptions(JsonTextWriter writer, JsonSerializer serializer, PlatformExportManifest manifest,
            ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            progressInfo.Description = "Exporting options...";
            progressCallback(progressInfo);

            var totalCount = _optionSearchService.Search(new ThumbnailOptionSearchCriteria {Take = 0, Skip = 0})
                .TotalCount;
            writer.WritePropertyName("OptionsTotalCount");
            writer.WriteValue(totalCount);

            writer.WritePropertyName("Options");
            writer.WriteStartArray();

            for (int i = 0; i < totalCount; i += _batchSize)
            {
                var options = _optionSearchService
                    .Search(new ThumbnailOptionSearchCriteria {Take = _batchSize, Skip = i}).Results;

                foreach (var option in options)
                {
                    serializer.Serialize(writer, option);
                }

                writer.Flush();
                progressInfo.Description = $"{Math.Min(totalCount, i + _batchSize)} of {totalCount} options exported";
                progressCallback(progressInfo);
            }

            writer.WriteEndArray();
        }

        private void ExportTasks(JsonTextWriter writer, JsonSerializer serializer, PlatformExportManifest manifest,
            ExportImportProgressInfo progressInfo, Action<ExportImportProgressInfo> progressCallback)
        {
            progressInfo.Description = "Exporting tasks...";
            progressCallback(progressInfo);

            var totalCount = _taskSearchService.Search(new ThumbnailTaskSearchCriteria() {Take = 0, Skip = 0})
                .TotalCount;
            writer.WritePropertyName("TakskTotalCount");
            writer.WriteValue(totalCount);

            writer.WritePropertyName("Tasks");
            writer.WriteStartArray();

            for (int i = 0; i < totalCount; i += _batchSize)
            {
                var tasks = _taskSearchService.Search(new ThumbnailTaskSearchCriteria {Take = _batchSize, Skip = i})
                    .Results;

                foreach (var task in tasks)
                {
                    serializer.Serialize(writer, task);
                }

                writer.Flush();
                progressInfo.Description = $"{Math.Min(totalCount, i + _batchSize)} of {totalCount} tasks exported";
                progressCallback(progressInfo);
            }

            writer.WriteEndArray();
        }


        public void DoImport(Stream inputStream, PlatformExportManifest manifest,
            Action<ExportImportProgressInfo> progressCallback)
        {
            var progressInfo = new ExportImportProgressInfo();
            using (StreamReader streamReader = new StreamReader(inputStream))
            {
                using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
                {
                    while (jsonReader.Read())
                    {
                        if (jsonReader.TokenType != JsonToken.PropertyName)
                            continue;

                        switch (jsonReader.Value.ToString())
                        {
                            case "Options":
                                jsonReader.Read();
                                var options = _serializer.Deserialize<ThumbnailOption[]>(jsonReader);
                                progressInfo.Description = $"Importing {options.Length} options...";
                                progressCallback(progressInfo);
                                _optionService.SaveOrUpdate(options);
                                break;
                            case "Tasks":
                                jsonReader.Read();
                                var tasks = _serializer.Deserialize<ThumbnailTask[]>(jsonReader);
                                progressInfo.Description = $"Importing {tasks.Length} tasks...";
                                progressCallback(progressInfo);
                                _taskService.SaveOrUpdate(tasks);
                                break;
                        }
                    }
                }
            }
        }
    }
}