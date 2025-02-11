using System.Globalization;
using System.Reflection;
using CsvHelper;
using Microsoft.ML;
using NLPHelpDesk.Data.Models;

namespace NLPHelpDesk.Trainer
{
    /// <summary>
    /// This class is responsible for training the machine learning models for category and priority prediction.
    /// </summary>
    class Program
    {
        private static readonly string _categoryModelFileName = "category_model.zip";
        private static readonly string _priorityModelFileName = "priority_model.zip";
        
        /// <summary>
        /// Main entry point for the model training application.
        /// </summary>
        /// <param name="args">Command line arguments (not used).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting model training...");

            // Read training data from CSV file.
            var data = ReadCsvData();

            // Check if data was loaded successfully.
            if (data == null || !data.Any())
            {
                Console.WriteLine("Error: No training data found.");
                return;
            }

            // Create MLContext for ML.NET operations.
            MLContext mlContext = new MLContext(seed: 0);

            // Load data into IDataView for ML.NET.
            IDataView dataView = mlContext.Data.LoadFromEnumerable(data);
            
            // Train the category prediction model.
            var categoryModel = GetCategoryModel(mlContext, dataView);
            if (categoryModel == null)
            {
                Console.WriteLine("Error: Fail to train category model.");
                return;
            }
            
            // Train the priority prediction model.
            var priorityModel = GetPriorityModel(mlContext, dataView);
            if (priorityModel == null)
            {
                Console.WriteLine("Error: Fail to train priority model.");
                return;
            }
            
            // Get the root directory of the solution.
            string currentDir = Directory.GetCurrentDirectory();
            string solutionRoot = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", ".."));
            string modelFolderPath = Path.Combine(solutionRoot, "NLPHelpDesk", "MLModels");

            // Create the model save paths.
            string categoryModelPath = Path.Combine(modelFolderPath, _categoryModelFileName);
            string priorityModelPath = Path.Combine(modelFolderPath, _priorityModelFileName);

            // Save the trained models.
            mlContext.Model.Save(categoryModel, dataView.Schema, categoryModelPath);
            mlContext.Model.Save(priorityModel, dataView.Schema, priorityModelPath);
            
            Console.WriteLine("Model training complete.");
        }

        /// <summary>
        /// Reads CSV data from an embedded resource file.
        /// </summary>
        /// <returns>A list of CsvData objects, or null if an error occurs.</returns>
        private static List<CsvData> ReadCsvData()
        {
            var data = new List<CsvData>();
            
            // Access the embedded CSV resource.
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NLPHelpDesk.Trainer.Data.helpdesk_dataset.csv");
            if (stream == null)
            {
                Console.WriteLine(stream);
                Console.WriteLine("Error: Embedded CSV file not found.");
                return null;
            }

            try
            {
                // Start reading csv file
                using (var reader = new StreamReader(stream))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        // Reading csv file by row
                        csv.Read();
                        // Reading header
                        csv.ReadHeader();
                        while (csv.Read())
                        {
                            // Mapping csv columns into CsvData
                            var question = csv.GetField<string>("Question");
                            var answer = csv.GetField<string>("Answer");
                            var category = csv.GetField<string>("Category");
                            var difficulty = csv.GetField<string>("Difficulty");

                            // Convert Difficulty to Priority
                            string priority = difficulty switch
                            {
                                "Basic" => "Low",
                                "Intermediate" => "Medium",
                                "Advanced" => "High",
                                _ => "Low"
                            };
                        
                            // Add new csv data to list
                            data.Add(new CsvData
                            {
                                Text = question,
                                Category = category,
                                Priority = priority
                            });
                        }
                    }
                }
            
                // Return list of CsvData
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV: {ex.Message}"); // Log the exception message.
                return new List<CsvData>();
            }
        }

        /// <summary>
        /// Trains the model for category prediction.
        /// </summary>
        /// <param name="mlContext">The MLContext instance.</param>
        /// <param name="dataView">The training data.</param>
        /// <returns>The trained transformer model.</returns>
        private static ITransformer GetCategoryModel(MLContext mlContext, IDataView dataView)
        {
            Console.WriteLine("Start model training for category.");
            
            var pipeline = mlContext.Transforms.Text.FeaturizeText(inputColumnName: "Text", outputColumnName: "Features") // Featurize the text column
                .Append(mlContext.Transforms.NormalizeMinMax(inputColumnName: "Features",
                    outputColumnName: "NormalizedFeatures"))// Normalize features
                .Append(mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Category",
                    outputColumnName: "Label"))// Map category values to keys for training
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label",
                    featureColumnName: "NormalizedFeatures"))// Train a multiclass classification model
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(inputColumnName: "PredictedLabel",
                    outputColumnName: "PredictedCategory")); // Map predicted keys back to category values
            
            var model = pipeline.Fit(dataView);

            return model;
        }

        /// <summary>
        /// Trains the model for priority prediction.
        /// </summary>
        /// <param name="mlContext">The MLContext instance.</param>
        /// <param name="dataView">The training data.</param>
        /// <returns>The trained transformer model.</returns>
        private static ITransformer GetPriorityModel(MLContext mlContext, IDataView dataView)
        {
            Console.WriteLine("Start model training for priority.");
            
            var pipeline = mlContext.Transforms.Text 
                .FeaturizeText(inputColumnName: "Text", outputColumnName: "Features") // Featurize the text column
                .Append(mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Priority",
                    outputColumnName: "Label")) // Map priority values to keys for training
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label",
                    featureColumnName: "Features")) // Train a multiclass classification model
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(inputColumnName: "PredictedLabel",
                    outputColumnName: "PredictedPriority")); // Map predicted keys back to priority values
        
            // Train the model.
            var model = pipeline.Fit(dataView);

            // Return model
            return model;
        }
    }
}