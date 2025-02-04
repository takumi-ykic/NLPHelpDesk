namespace NLPHelpDesk.Interfaces;

/// <summary>
/// Defines the interface for a service that manages user-related operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Checks if a user with the given ID exists.
    /// </summary>
    /// <param name="userId">The ID of the user to check.</param>
    /// <returns>A task that represents the asynchronous operation..</returns>
    Task<bool> IsExistingUser(string userId);
}