using NLPHelpDesk.Models;

namespace NLPHelpDesk.Interfaces;

/// <summary>
/// Defines the interface for a service that predicts the priority of a given data input
/// based on a set of training data.
/// </summary>
public interface IPriorityPredictionService
{
    /// <summary>
    /// Predicts the priority of a given input data based on a set of training data.
    /// </summary>
    /// <param name="data">A list of CsvData objects representing the training data.</param>
    /// <param name="inputData">A CsvData object representing the input data for which to predict the priority.</param>
    /// <returns>A PriorityPrediction object representing the predicted priority and any associated information. Returns null if a prediction could not be made.</returns>
    Task<PriorityPrediction> GetPriorityPrediction(List<CsvData> data, CsvData inputData);
}