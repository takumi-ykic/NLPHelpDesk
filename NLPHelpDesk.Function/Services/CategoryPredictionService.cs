using Microsoft.Extensions.Logging;
using Microsoft.ML;
using NLPHelpDesk.Data.Models;
using NLPHelpDesk.Function.Interfaces;

namespace NLPHelpDesk.Function.Services;

/// <summary>
/// Implements the <see cref="NLPHelpDesk.Function.Interfaces.ICategoryPredictionService"/> interface to provide ticket category predictions using machine learning.
/// </summary>
public class CategoryPredictionService : ICategoryPredictionService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<CategoryPredictionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryPredictionService"/> class.
    /// </summary>
    /// <param name="logger">The logger for logging messages.</param>
    /// <param name="memoryCache">The memory cache to store the trained model.</param>
    public CategoryPredictionService(ILogger<CategoryPredictionService> logger)
    {
        _mlContext = new MLContext(seed: 0);
        _logger = logger;
    }

    /// <summary>
    /// Gets a category prediction for a given input data.
    /// </summary>
    /// <param name="inputData">The input data as a <see cref="CsvData"/> object for which the prediction is to be made.</param>
    /// <returns>A <see cref="CategoryPrediction"/> object containing the predicted category, or null if an error occurs.</returns>
    public async Task<CategoryPrediction> GetCategoryPrediction(CsvData inputData)
    {
        var model = await GetModelAsync();
        if (model == null)
        {
            return null;
        }
        
        // Create the prediction engine.
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<CsvData, CategoryPrediction>(model, ignoreMissingColumns: true);
        
        // Make the prediction using the cached prediction engine.
        return predictionEngine.Predict(inputData);
    }

    /// <summary>
    /// Trains the machine learning model for category prediction.
    /// </summary>
    /// <returns>The trained <see cref="ITransformer"/> model, or null if training fails.</returns>
    private async Task<ITransformer> GetModelAsync()
    {
        string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLModels", "category_model.zip");

        try
        {
            // Attempt to load the model. The 'out modelSchema' parameter receives the schema of the loaded model.
            DataViewSchema modelSchema;
            var model = _mlContext.Model.Load(modelPath, out modelSchema);
            _logger.LogInformation("Model loaded successfully.");
            return model;
        }
        catch (FileNotFoundException ex) // Catch specific exceptions for better error handling
        {
            _logger.LogError(ex, "Model file not found at: {ModelPath}", modelPath);
            return null;
        }
        catch (InvalidDataException ex)  // For corrupted model files
        {
            _logger.LogError(ex, "Invalid model file format at: {ModelPath}", modelPath);
            return null;
        }
        catch (Exception ex) // Catching a general exception is still important
        {
            _logger.LogError(ex, "Error loading the model from: {ModelPath}", modelPath);
            return null;
        }
    }
}