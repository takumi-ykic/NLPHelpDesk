using Microsoft.Extensions.Caching.Memory;
using Microsoft.ML;
using NLPHelpDesk.Interfaces;
using NLPHelpDesk.Models;
using static NLPHelpDesk.Helpers.Constants;


namespace NLPHelpDesk.Services;

/// <summary>
/// Implements the <see cref="ICategoryPredictionService"/> interface to provide ticket category predictions using machine learning.
/// </summary>
public class CategoryPredictionService : ICategoryPredictionService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<CategoryPredictionService> _logger;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryPredictionService"/> class.
    /// </summary>
    /// <param name="logger">The logger for logging messages.</param>
    /// <param name="memoryCache">The memory cache to store the trained model.</param>
    public CategoryPredictionService(ILogger<CategoryPredictionService> logger, IMemoryCache memoryCache)
    {
        _mlContext = new MLContext(seed: 0);
        _logger = logger;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Gets a category prediction for a given input data.
    /// </summary>
    /// <param name="data">The training data as a list of <see cref="CsvData"/> objects.</param>
    /// <param name="inputData">The input data as a <see cref="CsvData"/> object for which the prediction is to be made.</param>
    /// <returns>A <see cref="CategoryPrediction"/> object containing the predicted category, or null if an error occurs.</returns>
    public async Task<CategoryPrediction> GetCategoryPrediction(List<CsvData> data, CsvData inputData)
    {
        // Check if the prediction engine is already in the cache.
        if (!_memoryCache.TryGetValue(CACHE_KEY_CATEGORY_MODEL,
                out PredictionEngine<CsvData, CategoryPrediction> predictionEngine))
        {
            // Train the model if it's not in the cache.
            var model = await GetModelAsync(data);
            if (model == null)
            {
                return null;
            }

            // Create the prediction engine.
            predictionEngine = _mlContext.Model.CreatePredictionEngine<CsvData, CategoryPrediction>(model, ignoreMissingColumns: true);

            // Store the prediction engine in the cache with a sliding expiration of 180 days.
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromDays(180));
            _memoryCache.Set(CACHE_KEY_CATEGORY_MODEL, predictionEngine, cacheEntryOptions);
        }
        
        // Make the prediction using the cached prediction engine.
        return predictionEngine.Predict(inputData);
    }

    /// <summary>
    /// Trains the machine learning model for category prediction.
    /// </summary>
    /// <param name="data">The training data as a list of <see cref="CsvData"/> objects.</param>
    /// <returns>The trained <see cref="ITransformer"/> model, or null if training fails.</returns>
    private async Task<ITransformer> GetModelAsync(List<CsvData> data)
    {
        // Load the data into an IDataView.
        IDataView dataView = _mlContext.Data.LoadFromEnumerable(data);

        var pipeline = _mlContext.Transforms.Text.FeaturizeText(inputColumnName: "Text", outputColumnName: "Features") // Featurize the text column
            .Append(_mlContext.Transforms.NormalizeMinMax(inputColumnName: "Features",
                outputColumnName: "NormalizedFeatures"))// Normalize features
            .Append(_mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Category",
                outputColumnName: "Label"))// Map category values to keys for training
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label",
                featureColumnName: "NormalizedFeatures"))// Train a multiclass classification model
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue(inputColumnName: "PredictedLabel",
                outputColumnName: "PredictedCategory")); // Map predicted keys back to category values
        
        // Train the model.
        var model = pipeline.Fit(dataView);

        // Return model
        return model;
    }
}