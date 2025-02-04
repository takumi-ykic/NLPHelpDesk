using NLPHelpDesk.Models;

namespace NLPHelpDesk.Interfaces;

/// <summary>
/// Defines the interface for a service that predicts the category of a given data input
/// based on a set of training data.
/// </summary>
interface ICategoryPredictionService
{
    /// <summary>
    /// Predicts the category of a given input data based on a set of training data.
    /// </summary>
    /// <param name="data">A list of CsvData objects representing the training data.</param>
    /// <param name="inputData">A CsvData object representing the input data for which to predict the category.</param>
    /// <returns>A CategoryPrediction object representing the predicted category and any associated information. Returns null if a prediction could not be made.</returns>
    Task<CategoryPrediction> GetCategoryPrediction(List<CsvData> data, CsvData inputData);
}