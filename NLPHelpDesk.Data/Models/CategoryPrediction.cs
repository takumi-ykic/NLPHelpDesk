namespace NLPHelpDesk.Data.Models;

/// <summary>
/// Represents the result of a category prediction.
/// </summary>
public class CategoryPrediction
{
    public string? PredictedCategory { get; set; }
    public float[]? Scores { get; set; }
}