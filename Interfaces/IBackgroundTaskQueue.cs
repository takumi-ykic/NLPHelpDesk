namespace NLPHelpDesk.Interfaces;

/// <summary>
/// Defines an interface for a background task queue.  This queue allows you to 
/// enqueue work items that will be processed in the background.
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// Queues a background work item for processing.
    /// </summary>
    /// <param name="workItem">The work item to queue. This is a function that 
    /// takes an IServiceProvider and returns a Task.</param>
    /// <remarks>
    /// The IServiceProvider can be used to resolve dependencies required by the 
    /// background task.  This is crucial for dependency injection.
    /// </remarks>
    void QueueBackgroundWorkItem(Func<IServiceProvider, Task> workItem);
    
    /// <summary>
    /// Dequeues a background work item from the queue.  This method will block 
    /// until a work item is available.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel 
    /// the dequeue operation.</param>
    /// <returns>A function representing the dequeued work item, or null if the 
    /// cancellation token is signaled.</returns>
    Task<Func<IServiceProvider, Task>?> DequeueAsync(CancellationToken cancellationToken);
}