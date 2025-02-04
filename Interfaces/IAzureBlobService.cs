using Microsoft.AspNetCore.Mvc;

namespace NLPHelpDesk.Interfaces;

/// <summary>
/// Defines the interface for a service that interacts with Azure Blob Storage.
/// </summary>
public interface IAzureBlobService
{
    /// <summary>
    /// Uploads a file to Azure Blob Storage.
    /// </summary>
    /// <param name="fileName">The name of the file to upload.</param>
    /// <param name="file">The IFormFile representing the file to upload.</param>
    /// <returns>A task that represents the asynchronous operation. Returns true if the upload was successful, false otherwise.</returns>
    Task<bool> UploadFileToAzureBlob(string fileName, IFormFile file);
    
    /// <summary>
    /// Downloads a file from Azure Blob Storage.
    /// </summary>
    /// <param name="fileName">The name of the file to download.</param>
    /// <returns>A task that represents the asynchronous operation. Returns a <see cref="FileContentResult"/> containing the file content and content type, or null if the file is not found or an error occurs.</returns>
    Task<FileContentResult> DownloadFileFromAzureBlob(string fileName);
    
    /// <summary>
    /// Generates a Shared Access Signature (SAS) URI for a blob (file) in Azure Blob Storage.
    /// </summary>
    /// <param name="fileName">The name of the blob.</param>
    /// <returns>A task that represents the asynchronous operation. Returns the SAS URI as a string, or null if an error occurs.</returns>
    Task<string> GetBlobSasUri(string fileName);
}