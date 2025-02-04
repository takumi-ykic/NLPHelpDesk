using System.Collections.Concurrent;
using NLPHelpDesk.Interfaces;

namespace NLPHelpDesk.Services;

/// <summary>
/// Implements a background task queue using a concurrent queue.
/// </summary>
public class BackgroundTaskQueue: IBackgroundTaskQueue
{
    private readonly ConcurrentQueue<Func<IServiceProvider, Task>> _workItems = new();
    
    /// <summary>
    /// Queues a background work item.
    /// </summary>
    /// <param name="workItem">The work item to queue.</param>
    public void QueueBackgroundWorkItem(Func<IServiceProvider, Task> workItem)
    {
        _workItems.Enqueue(workItem);
    }

    /// <summary>
    /// Dequeues a background work item. This method will block until an item is available or the cancellation token is signaled.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to stop the dequeue operation.</param>
    /// <returns>The dequeued work item, or null if the cancellation token is signaled before an item is available.</returns>
    public async Task<Func<IServiceProvider, Task>?> DequeueAsync(CancellationToken cancellationToken)
    {
        // Keep trying to dequeue until an item is available or cancellation is requested.
        while (_workItems.TryDequeue(out var workItem))
        {
            // Return the work item if one is available.
            return workItem;
        }

        // If no item is available, wait for a short period or until cancellation.
        await Task.Delay(100, cancellationToken);
        
        // Return null if no item is available and cancellation was requested.
        return null;
    }
}