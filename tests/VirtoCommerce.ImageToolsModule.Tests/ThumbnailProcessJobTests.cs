using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.ImageToolsModule.Core.Models;
using VirtoCommerce.ImageToolsModule.Core.PushNotifications;
using VirtoCommerce.ImageToolsModule.Core.Services;
using VirtoCommerce.ImageToolsModule.Core.ThumbnailGeneration;
using VirtoCommerce.ImageToolsModule.Data.BackgroundJobs;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Platform.Core.PushNotifications;
using Xunit;

namespace VirtoCommerce.ImageToolsModule.Tests
{
    public class ThumbnailProcessJobTests
    {
        private readonly Mock<IPushNotificationManager> _pushNotifier = new();
        private readonly Mock<IThumbnailGenerationProcessor> _processor = new();
        private readonly Mock<IThumbnailTaskService> _taskService = new();
        private readonly Mock<IThumbnailTaskSearchService> _taskSearchService = new();
        private readonly Mock<IDistributedLockService> _lockService = new();

        // Values of the notification at the moment the final (finished) push is sent.
        private long _finalTotalCount;
        private long _finalProcessedCount;
        private List<string> _finalErrors;

        public ThumbnailProcessJobTests()
        {
            _lockService
                .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<Func<Task<bool>>>(), It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken?>()))
                .Returns<string, Func<Task<bool>>, TimeSpan?, TimeSpan?, TimeSpan?, CancellationToken?>((_, resolver, _, _, _, _) => resolver());

            _pushNotifier
                .Setup(x => x.SendAsync(It.IsAny<PushNotification>()))
                .Callback<PushNotification>(n =>
                {
                    var notification = (ThumbnailProcessNotification)n;
                    _finalTotalCount = notification.TotalCount;
                    _finalProcessedCount = notification.ProcessedCount;
                    _finalErrors = notification.Errors.ToList();
                })
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Process_SingleTask_FinalNotificationReportsProcessedImages()
        {
            // Arrange
            SetupTasks(new TaskRun("task1", 3));
            var job = CreateJob();

            // Act
            await job.Process(new ThumbnailsTaskRunRequest { TaskIds = ["task1"] }, null, null, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(3, _finalTotalCount);
            Assert.Equal(3, _finalProcessedCount);
        }

        [Fact]
        public async Task Process_SeveralTasks_FinalNotificationSumsAllTasksAndKeepsErrors()
        {
            // Arrange
            SetupTasks(new TaskRun("task1", 3, "error in task1"), new TaskRun("task2", 2));
            var job = CreateJob();

            // Act
            await job.Process(new ThumbnailsTaskRunRequest { TaskIds = ["task1", "task2"] }, null, null, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(5, _finalTotalCount);
            Assert.Equal(5, _finalProcessedCount);
            Assert.Equal(["error in task1"], _finalErrors);
        }

        private ThumbnailProcessJob CreateJob()
        {
            return new ThumbnailProcessJob(_pushNotifier.Object, _processor.Object, _taskService.Object, _taskSearchService.Object, _lockService.Object);
        }

        private void SetupTasks(params TaskRun[] runs)
        {
            var tasks = runs.Select(x => new ThumbnailTask { Id = x.TaskId, Name = x.TaskId }).ToList();

            _taskService
                .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(tasks);

            // Mimics ThumbnailGenerationProcessor: every call reports its own task only, through a progress object of its own.
            _processor
                .Setup(x => x.ProcessTasksAsync(It.IsAny<ICollection<ThumbnailTask>>(), It.IsAny<bool>(), It.IsAny<Action<ThumbnailTaskProgress>>(), It.IsAny<CancellationToken>()))
                .Callback<ICollection<ThumbnailTask>, bool, Action<ThumbnailTaskProgress>, CancellationToken>((taskBatch, _, progressCallback, _) =>
                {
                    var run = runs.Single(x => x.TaskId == taskBatch.Single().Id);
                    var progress = new ThumbnailTaskProgress { Message = $"Processing task {run.TaskId}..." };
                    progressCallback(progress);

                    progress.TotalCount = run.ImageCount;
                    progress.Errors.AddRange(run.Errors);
                    progress.ProcessedCount = run.ImageCount;
                    progressCallback(progress);
                })
                .Returns(Task.CompletedTask);
        }

        private sealed record TaskRun(string TaskId, int ImageCount, params string[] Errors);
    }
}
