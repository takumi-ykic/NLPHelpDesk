using System.Reflection;
using Microsoft.ML;
using NLPHelpDesk.Interfaces;
using NLPHelpDesk.Data.Models;


namespace NLPHelpDesk.Services;

/// <summary>
/// Implements the <see cref="ICategoryPredictionService"/> interface to provide ticket category predictions using machine learning.
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
    /// <param name="data">The training data as a list of <see cref="CsvData"/> objects.</param>
    /// <returns>The trained <see cref="ITransformer"/> model, or null if training fails.</returns>
    private async Task<ITransformer> GetModelAsync()
    {
        string modelPath;

        // Check if running locally or in Azure
        if (Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development")
        {
            // For local development, model is in the MLModels folder
            modelPath = Path.Combine(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..")), "NLPHelpDesk", "MLModels", "category_model.zip");
        }
        else
        {
            // For Azure, model will be extracted to the HOME directory
            modelPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "category_model.zip");
        }
        
        // Get path for model
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = "NLPHelpDesk.MLModels.category_model.zip";

        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                _logger.LogError("Model resource not found in the assembly.");
                return null;
            }
            
            using (var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
            }
        }
        
        try
        {
            DataViewSchema modelSchema;
            var model = _mlContext.Model.Load(modelPath, out modelSchema);
            _logger.LogInformation("Model loaded successfully.");
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading the model.");
            return null;
        }
    }
}