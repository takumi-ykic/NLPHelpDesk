namespace NLPHelpDesk.Data.Models;

/// <summary>
/// Represents an error view model.
/// </summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}