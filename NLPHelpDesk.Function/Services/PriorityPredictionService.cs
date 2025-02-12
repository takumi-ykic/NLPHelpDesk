using Microsoft.Extensions.Logging;
using Microsoft.ML;
using NLPHelpDesk.Data.Models;
using NLPHelpDesk.Function.Interfaces;

namespace NLPHelpDesk.Function.Services;

/// <summary>
/// Implements the <see cref="NLPHelpDesk.Function.Interfaces.IPriorityPredictionService"/> interface to provide ticket priority predictions using machine learning.
/// </summary>
public class PriorityPredictionService : IPriorityPredictionService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<PriorityPredictionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PriorityPredictionService"/> class.
    /// </summary>
    /// <param name="memoryCache">The memory cache to store the trained model.</param>
    public PriorityPredictionService(ILogger<PriorityPredictionService> logger)
    {
        _mlContext = new MLContext(seed: 0);
        _logger = logger;
    }
    
    /// <summary>
    /// Gets a priority prediction for a given input data.
    /// </summary>
    /// <param name="inputData">The input data as a <see cref="CsvData"/> object for which the prediction is to be made.</param>
    /// <returns>A <see cref="PriorityPrediction"/> object containing the predicted priority, or null if an error occurs.</returns>
    public async Task<PriorityPrediction> GetPriorityPrediction(CsvData inputData)
    {
        var model = await GetModelAsync();
        if (model == null)
        {
            return null;
        }
        
        // Create the prediction engine.
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<CsvData, PriorityPrediction>(model, ignoreMissingColumns: true);
        
        // Make the prediction using the cached prediction engine.
        return predictionEngine.Predict(inputData);
    }

    /// <summary>
    /// Trains the machine learning model for priority prediction.
    /// </summary>
    /// <param name="data">The training data as a list of <see cref="CsvData"/> objects.</param>
    /// <returns>The trained <see cref="ITransformer"/> model, or null if training fails.</returns>
    private async Task<ITransformer> GetModelAsync()
    {
        // string modelPath;
        //
        // // Check if running locally or in Azure
        // if (Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") == "Development")
        // {
        //     // For local development, model is in the MLModels folder
        //     modelPath = Path.Combine(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..")), "NLPHelpDesk", "MLModels", "priority_model.zip");
        // }
        // else
        // {
        //     // For Azure, model will be extracted to the HOME directory
        //     modelPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "priority_model.zip");
        // }
        //
        // // Get path for model
        // var assembly = Assembly.GetExecutingAssembly();
        // string resourceName = "NLPHelpDesk.MLModels.priority_model.zip";
        //
        // using (var stream = assembly.GetManifestResourceStream(resourceName))
        // {
        //     if (stream == null)
        //     {
        //         _logger.LogError("Model resource not found in the assembly.");
        //         return null;
        //     }
        //     
        //     using (var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write))
        //     {
        //         await stream.CopyToAsync(fileStream);
        //     }
        // }
        //
        // try
        // {
        //     DataViewSchema modelSchema;
        //     var model = _mlContext.Model.Load(modelPath, out modelSchema);
        //     _logger.LogInformation("Model loaded successfully.");
        //     return model;
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Error loading the model.");
        //     return null;
        // }
        
        string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MLModels", "category_model.zip");

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