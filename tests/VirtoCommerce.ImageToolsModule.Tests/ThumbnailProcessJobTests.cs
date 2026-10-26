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

namespace VirtoCommerce.ImageToolsModule.Tests;

public class ThumbnailProcessJobTests
{
    private const string SuccessDescription = "Thumbnails generated successfully!";

    private readonly Mock<IPushNotificationManager> _pushNotifier = new();
    private readonly Mock<IThumbnailGenerationProcessor> _processor = new();
    private readonly Mock<IThumbnailTaskService> _taskService = new();
    private readonly Mock<IThumbnailTaskSearchService> _taskSearchService = new();
    private readonly Mock<IDistributedLockService> _lockService = new();

    // Every push, in order, as the notification looked at the moment it was sent. The last one is the final (finished) push.
    private readonly List<SentNotification> _sent = [];

    public ThumbnailProcessJobTests()
    {
        _lockService
            .Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<Func<Task<bool>>>(), It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken?>()))
            .Returns<string, Func<Task<bool>>, TimeSpan?, TimeSpan?, TimeSpan?, CancellationToken?>((_, resolver, _, _, _, _) => resolver());

        _pushNotifier
            .Setup(x => x.Send(It.IsAny<PushNotification>()))
            .Callback<PushNotification>(Capture);

        _pushNotifier
            .Setup(x => x.SendAsync(It.IsAny<PushNotification>()))
            .Callback<PushNotification>(Capture)
            .Returns(Task.CompletedTask);
    }

    private SentNotification Final => _sent[^1];

    [Fact]
    public async Task Process_SingleTask_FinalNotificationReportsProcessedImages()
    {
        // Arrange
        SetupTasks(new TaskRun("task1", 3));

        // Act
        await RunAsync("task1");

        // Assert
        Assert.Equal(3, Final.TotalCount);
        Assert.Equal(3, Final.ProcessedCount);
        Assert.Equal(SuccessDescription, Final.Description);
    }

    [Fact]
    public async Task Process_SeveralTasks_FinalNotificationSumsAllTasksAndKeepsErrors()
    {
        // Arrange
        SetupTasks(new TaskRun("task1", 3, ["error in task1"]), new TaskRun("task2", 2, ["error in task2"]));

        // Act
        await RunAsync("task1", "task2");

        // Assert
        Assert.Equal(5, Final.TotalCount);
        Assert.Equal(5, Final.ProcessedCount);
        Assert.Equal(["error in task1", "error in task2"], Final.Errors);
        Assert.Equal("Thumbnail generation process completed with errors. 2 issues need your attention.", Final.Description);
    }

    [Fact]
    public async Task Process_RunWithErrors_NoNotificationReportsSuccess()
    {
        // Arrange
        SetupTasks(new TaskRun("task1", 3, ["error in task1"]));

        // Act
        await RunAsync("task1");

        // Assert
        Assert.DoesNotContain(_sent, x => x.Description == SuccessDescription);
    }

    [Fact]
    public async Task Process_LastTaskHasNoChanges_FinalNotificationKeepsEarlierTotals()
    {
        // Arrange
        SetupTasks(new TaskRun("task1", 3), new TaskRun("task2", 0));

        // Act
        await RunAsync("task1", "task2");

        // Assert
        Assert.Equal(3, Final.TotalCount);
        Assert.Equal(3, Final.ProcessedCount);
        Assert.Equal(SuccessDescription, Final.Description);
    }

    [Fact]
    public async Task Process_CanceledDuringSecondTask_FinalNotificationReportsRunProgress()
    {
        // Arrange
        SetupTasks(new TaskRun("task1", 3), new TaskRun("task2", 2, CanceledAfter: 1));

        // Act
        await RunAsync("task1", "task2");

        // Assert
        Assert.Equal(5, Final.TotalCount);
        Assert.Equal(4, Final.ProcessedCount);
        Assert.Equal("Thumbnail generation was canceled. Processed 4 of 5 images.", Final.Description);
    }

    private Task RunAsync(params string[] taskIds)
    {
        var job = new ThumbnailProcessJob(_pushNotifier.Object, _processor.Object, _taskService.Object, _taskSearchService.Object, _lockService.Object);

        return job.Process(new ThumbnailsTaskRunRequest { TaskIds = taskIds }, null, null, TestContext.Current.CancellationToken);
    }

    private void Capture(PushNotification pushNotification)
    {
        var notification = (ThumbnailProcessNotification)pushNotification;
        _sent.Add(new SentNotification(notification.Description, notification.TotalCount, notification.ProcessedCount, notification.Errors.ToList()));
    }

    private void SetupTasks(params TaskRun[] runs)
    {
        var tasks = runs.Select(x => new ThumbnailTask { Id = x.TaskId, Name = x.TaskId }).ToList();

        _taskService
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(tasks);

        // Mimics ThumbnailGenerationProcessor: every call reports its own task only, through a progress object of its own,
        // and stops processing a task that has no changes.
        _processor
            .Setup(x => x.ProcessTasksAsync(It.IsAny<ICollection<ThumbnailTask>>(), It.IsAny<bool>(), It.IsAny<Action<ThumbnailTaskProgress>>(), It.IsAny<CancellationToken>()))
            .Callback<ICollection<ThumbnailTask>, bool, Action<ThumbnailTaskProgress>, CancellationToken>((taskBatch, _, progressCallback, _) =>
            {
                var run = runs.Single(x => x.TaskId == taskBatch.Single().Id);
                var progress = new ThumbnailTaskProgress { Message = $"Processing task {run.TaskId}..." };
                progressCallback(progress);

                progress.TotalCount = run.ImageCount;
                if (run.ImageCount == 0)
                {
                    return;
                }

                progress.Errors.AddRange(run.Errors ?? []);
                progress.ProcessedCount = run.CanceledAfter ?? run.ImageCount;
                progressCallback(progress);

                if (run.CanceledAfter != null)
                {
                    throw new OperationCanceledException();
                }
            })
            .Returns(Task.CompletedTask);
    }

    private sealed record TaskRun(string TaskId, int ImageCount, string[] Errors = null, int? CanceledAfter = null);

    private sealed record SentNotification(string Description, long TotalCount, long ProcessedCount, List<string> Errors);
}
