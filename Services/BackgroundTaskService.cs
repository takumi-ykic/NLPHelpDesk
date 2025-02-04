using NLPHelpDesk.Interfaces;

namespace NLPHelpDesk.Services;

/// <summary>
/// A background task service that processes tasks from a queue.
/// </summary>
public class BackgroundTaskService: BackgroundService
{
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundTaskService"/> class.
    /// </summary>
    /// <param name="backgroundTaskQueue">The background task queue.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public BackgroundTaskService(IBackgroundTaskQueue backgroundTaskQueue, IServiceProvider serviceProvider)
    {
        _backgroundTaskQueue = backgroundTaskQueue;
        _serviceProvider = serviceProvider;
    }
    
    /// <summary>
    /// Executes the background task service.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token that can be used to stop the service.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Dequeue a task from the queue. This will block until a task is available.
            var workItem = await _backgroundTaskQueue.DequeueAsync(stoppingToken);
            if (workItem != null)
            {
                // Create a new scope for the task.
                using var scope = _serviceProvider.CreateScope();
                
                // Execute the task within the scope.
                await workItem(scope.ServiceProvider);
            }
        }
    }
}