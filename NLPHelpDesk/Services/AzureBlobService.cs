using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using NLPHelpDesk.Interfaces;
using static NLPHelpDesk.Helpers.Constants;

namespace NLPHelpDesk.Services;

/// <summary>
/// Implements the <see cref="IAzureBlobService"/> interface to interact with Azure Blob Storage.
/// </summary>
public class AzureBlobService : IAzureBlobService
{
    private readonly ILogger<AzureBlobService> _logger;
    private readonly string? _blobPath;
    private readonly string? _fileContainer;
    private readonly string? _accountName;
    private readonly string? _accessKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobService"/> class.
    /// </summary>
    /// <param name="logger">The logger for logging messages.</param>
    /// <param name="configuration">The application configuration.</param>
    public AzureBlobService(ILogger<AzureBlobService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _blobPath = configuration.GetValue<string>(AZURE_BLOB_PATH);
        _fileContainer = configuration.GetValue<string>(AZURE_BLOB_CONTAINER_FILES);
        _accountName = configuration.GetValue<string>(AZURE_STORAGE_ACCOUNT_NAME);
        _accessKey = configuration.GetValue<string>(AZURE_BLOB_ACCESS_KEY);
    }
    
    /// <summary>
    /// Uploads a file to Azure Blob Storage.
    /// </summary>
    /// <param name="fileName">The name of the blob to create.</param>
    /// <param name="file">The file to upload.</param>
    /// <returns><c>true</c> if the upload was successful; otherwise, <c>false</c>.</returns>
    public async Task<bool> UploadFileToAzureBlob(string fileName, IFormFile file)
    {
        // Log an error if the file or fileName is null or empty.
        if (string.IsNullOrEmpty(fileName) || file.Length == 0)
        {
            _logger.LogError("File is null or empty");
            return false;
        }

        try
        {
            // Get the blob container client.
            var containerClient = await GetBlobContainerClient(_fileContainer);
            
            // Upload the file to the blob container.
            await containerClient.UploadBlobAsync(fileName, file.OpenReadStream());
            
            return true;
        }
        catch (Azure.RequestFailedException ex)
        {
            _logger.LogError(ex, $"Error uploading file: {fileName}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An unexpected error occurred uploading file: {fileName}");
            return false;
        }
    }

    /// <summary>
    /// Downloads a file from Azure Blob Storage.
    /// </summary>
    /// <param name="fileName">The name of the blob to download.</param>
    /// <returns>A <see cref="FileContentResult"/> containing the file content and content type, or null if an error occurs.</returns>
    public async Task<FileContentResult> DownloadFileFromAzureBlob(string fileName)
    {
        // Log an error if the fileName is null or empty.
        if (string.IsNullOrEmpty(fileName))
        {
            _logger.LogError("File is null or empty");
            return null;
        }
        
        try
        {
            // Get the blob container client.
            var containerClient = await GetBlobContainerClient(_fileContainer);
            
            // Get the blob client.
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            if (blobClient == null)
            {
                _logger.LogError("Blob client is null");
                return null;
            }

            // Download the blob to a memory stream.
            using (var memoryStream = new MemoryStream())
            {
                await blobClient.DownloadToAsync(memoryStream);
                var contentType = "application/octet-stream";
                return new FileContentResult(memoryStream.ToArray(), contentType)
                {
                    // Set the file download name
                    FileDownloadName = fileName
                };
            }
        }
        catch (Azure.RequestFailedException ex)
        {
            _logger.LogError(ex, $"Error downloading file: {fileName}");
            throw new Azure.RequestFailedException("An error occurred while downloading the file.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An unexpected error occurred downloading file: {fileName}");
            throw new Azure.RequestFailedException("An error occurred while downloading the file.", ex);
        }
    }

    
    /// <summary>
    /// Generates a Shared Access Signature (SAS) URI for a blob.
    /// </summary>
    /// <param name="fileName">The name of the blob.</param>
    /// <returns>The SAS URI as a string, or null if an error occurs.</returns>
    public async Task<string> GetBlobSasUri(string fileName)
    {
        // Log an error if the fileName is null or empty.
        if (string.IsNullOrEmpty(fileName))
        {
            _logger.LogError("File is null or empty");
            return null;
        }

        try
        {
            // Get the blob container client.
            var containerClient = await GetBlobContainerClient(_fileContainer);
            
            // Get the blob client.
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            if (blobClient == null)
            {
                _logger.LogError("Blob client is null");
                return null;
            }

            // Create a SAS builder.
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _fileContainer,
                BlobName = fileName,
                Resource = "b",
                ExpiresOn = DateTime.UtcNow.AddHours(1) // Set expiration time
            };

            // Set permissions
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            
            // Generate the SAS token
            var sasToken =
                sasBuilder.ToSasQueryParameters(new Azure.Storage.StorageSharedKeyCredential(_accountName, _accessKey));

            // Build the SAS URI
            BlobUriBuilder uriBuilder = new BlobUriBuilder(blobClient.Uri)
            {
                Sas = sasToken
            };

            // Return generated URI
            return uriBuilder.ToUri().ToString();
        }
        catch (Azure.RequestFailedException ex)
        {
            _logger.LogError(ex, $"Error downloading file: {fileName}");
            throw new Azure.RequestFailedException("An error occurred while generating token.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An unexpected error occurred while generating token.");
            throw new Azure.RequestFailedException("An error occurred while generating token.", ex);
        }
    }

    /// <summary>
    /// Gets a BlobContainerClient for the specified container name. Creates the container if it doesn't exist.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <returns>A <see cref="BlobContainerClient"/> object.</returns>
    private async Task<BlobContainerClient> GetBlobContainerClient(string containerName)
    {
        // Create a BlobServiceClient.
        BlobServiceClient serviceClient = new BlobServiceClient(_blobPath);
        
        // Get a BlobContainerClient.
        BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(containerName);
        
        // Create the container if it doesn't exist.
        if (!await containerClient.ExistsAsync())
        {
            await containerClient.CreateIfNotExistsAsync();
        }

        // Return CountainerClient
        return containerClient;
    }
}