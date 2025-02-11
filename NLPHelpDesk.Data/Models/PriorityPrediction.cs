namespace NLPHelpDesk.Data.Models;

/// <summary>
/// Represents the result of a priority prediction.
/// </summary>
public class PriorityPrediction
{
    public string? PredictedPriority { get; set; }
    public float[]? Scores { get; set; }
}