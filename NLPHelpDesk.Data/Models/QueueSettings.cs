namespace NLPHelpDesk.Data.Models;

/// <summary>
/// Represents the settings for configuring Azure Queue Storage.
/// </summary>
public class QueueSettings
{
    public string AzureStorageConnectionString { get; set; }
    public string PredictionQueue { get; set; }
}