using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;

namespace VirtoCommerce.ImageToolsModule.Data.ExportImport
{
    public class ThumbnailsExportImport
    {
        private readonly IThumbnailTaskSearchService _taskSearchService;
        private readonly IThumbnailOptionSearchService _optionSearchService;
        private readonly IThumbnailTaskService _taskService;
        private readonly IThumbnailOptionService _optionService;

        private const int _batchSize = 50;
        private readonly JsonSerializer _jsonSerializer;

        public ThumbnailsExportImport(IThumbnailTaskSearchService taskSearchService,
            IThumbnailOptionSearchService optionSearchService, IThumbnailTaskService taskService,
            IThumbnailOptionService optionService, JsonSerializer jsonSerializer)
        {
            _taskSearchService = taskSearchService;
            _optionSearchService = optionSearchService;
            _taskService = taskService;
            _optionService = optionService;
            _jsonSerializer = jsonSerializer;
        }

        public async Task DoExportAsync(Stream outStream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var progressInfo = new ExportImportProgressInfo { Description = "loading data..." };
            progressCallback(progressInfo);

            using (var sw = new StreamWriter(outStream, Encoding.UTF8))
            using (var writer = new JsonTextWriter(sw))
            {
                await writer.WriteStartObjectAsync();

                progressInfo.Description = "Options are started to export";
                progressCallback(progressInfo);

                await writer.WritePropertyNameAsync("Options");
                await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
                {
                    var searchCriteria = AbstractTypeFactory<ThumbnailOptionSearchCriteria>.TryCreateInstance();
                    searchCriteria.Take = take;
                    searchCriteria.Skip = skip;
                    var searchResult = await _optionSearchService.SearchNoCloneAsync(searchCriteria);
                    return (GenericSearchResult<ThumbnailOption>)searchResult;
                }, (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{processedCount} of {totalCount} Options have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                progressInfo.Description = "Tasks are started to export";
                progressCallback(progressInfo);

                await writer.WritePropertyNameAsync("Tasks");
                await writer.SerializeArrayWithPagingAsync(_jsonSerializer, _batchSize, async (skip, take) =>
                {
                    var searchCriteria = AbstractTypeFactory<ThumbnailTaskSearchCriteria>.TryCreateInstance();
                    searchCriteria.Take = take;
                    searchCriteria.Skip = skip;
                    var searchResult = await _taskSearchService.SearchNoCloneAsync(searchCriteria);
                    return (GenericSearchResult<ThumbnailTask>)searchResult;
                }, (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{processedCount} of {totalCount} Tasks have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                await writer.WriteEndObjectAsync();
                await writer.FlushAsync();
            }
        }

        public async Task DoImportAsync(Stream inputStream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var progressInfo = new ExportImportProgressInfo();
            using (var streamReader = new StreamReader(inputStream))
            using (var reader = new JsonTextReader(streamReader))
            {
                while (await reader.ReadAsync())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var readerValueString = reader.Value?.ToString();

                        if (readerValueString == "Options")
                        {
                            await reader.DeserializeArrayWithPagingAsync<ThumbnailOption>(_jsonSerializer, _batchSize, items => _optionService.SaveChangesAsync(items), processedCount =>
                            {
                                progressInfo.Description = $"{processedCount} Options have been imported";
                                progressCallback(progressInfo);
                            }, cancellationToken);
                        }
                        else if (readerValueString == "Tasks")
                        {
                            await reader.DeserializeArrayWithPagingAsync<ThumbnailTask>(_jsonSerializer, _batchSize, items => _taskService.SaveChangesAsync(items), processedCount =>
                            {
                                progressInfo.Description = $"{processedCount} Options have been imported";
                                progressCallback(progressInfo);
                            }, cancellationToken);
                        }
                    }
                }
            }
        }
    }
}
