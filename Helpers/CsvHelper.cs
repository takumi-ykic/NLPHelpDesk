using System.Globalization;
using CsvHelper;
using NLPHelpDesk.Models;

namespace NLPHelpDesk.Helpers;

/// <summary>
/// Provides methods for reading CSV data from a file.
/// </summary>
public class CsvHelper
{
    /// <summary>
    /// Reads CSV data from the "helpdesk_dataset.csv" file located in the "Data" directory.
    /// </summary>
    /// <returns>A list of CsvData objects representing the data in the CSV file. Returns an empty list if an error occurs or the file is not found.</returns>
    public static List<CsvData> ReadCsvData()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "helpdesk_dataset.csv");
        var data = new List<CsvData>();
        
        // Check if the path is valid.
        if (path == null)
        {
            return new List<CsvData>();
        }

        try
        {
            // Start reading csv file
            using (var reader = new StreamReader(path))
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
            // Return empty list
            return new List<CsvData>();
        }
    }
}