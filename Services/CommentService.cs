using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NLPHelpDesk.Contexts;
using NLPHelpDesk.Interfaces;
using NLPHelpDesk.Models;
using static NLPHelpDesk.Helpers.Constants;

namespace NLPHelpDesk.Services;

/// <summary>
/// Implements the <see cref="ICommentService"/> interface to manage comments.
/// </summary>
public class CommentService: ICommentService
{
    private readonly ApplicationContext _context;
    private readonly ILogger<CommentService> _logger;
    private readonly IAzureBlobService _blobService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="logger">The logger for logging messages.</param>
    /// <param name="blobService">The Azure blob service for file uploads.</param>
    public CommentService(ApplicationContext context,
        ILogger<CommentService> logger,
        IAzureBlobService blobService)
    {
        _context = context;
        _logger = logger;
        _blobService = blobService;
    }

    /// <summary>
    /// Retrieves a list of comments for a specific ticket.
    /// </summary>
    /// <param name="ticketId">The ID of the ticket.</param>
    /// <returns>A list of <see cref="Comment"/> objects.</returns>
    public async Task<List<Comment>> GetComments(string ticketId)
    {
        // Log a warning if the ticketId is invalid.
        if (string.IsNullOrWhiteSpace(ticketId))
        {
            _logger.LogWarning("Invalid or empty ticketId provided.");
            return new List<Comment>();
        }
        
        try
        {
            // Return comments associated with ticket ID
            return await _context.Comments
                .Where(c => c.TicketId == ticketId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "A SQL error occurred while retrieving comments.");
            return new List<Comment>();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "A database update error occurred while retrieving comments.");
            return new List<Comment>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            return new List<Comment>();
        }
    }

    /// <summary>
    /// Creates a new comment.
    /// </summary>
    /// <param name="userId">The ID of the user creating the comment.</param>
    /// <param name="ticketId">The ID of the ticket the comment belongs to.</param>
    /// <param name="commentText">The text of the comment.</param>
    /// <param name="file">An optional file to attach to the comment.</param>
    /// <returns>The created <see cref="Comment"/> object, or null if an error occurs.</returns>
    public async Task<Comment> CreateComment(string userId, string ticketId, string commentText, IFormFile? file)
    {
        // Log a warning if any required parameter is invalid.
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(ticketId) || string.IsNullOrEmpty(commentText))
        {
            _logger.LogWarning("Invalid or empty userId, ticketId, and commentText provided.");
            return null;
        }
        
        // Create new comment
        var comment = new Comment()
        {
            TicketId = ticketId,
            UserId = userId,
            CommentText = commentText.Trim(),
            CreateDate = DateTime.UtcNow
        };

        // Use a transaction for data consistency.
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // Handle file upload if a file is provided.
                if (file != null && file.Length > 0)
                {
                    // Store file to azure
                    // Validate file type and size.
                    var fileExtension = Path.GetExtension(file.FileName);
                    if (!ALLOWED_FILE_EXTENSION.Contains(fileExtension) || file.Length > MAX_FILE_SIZE)
                    {
                        _logger.LogWarning("File is not supported. Check file type and file size.");
                        return null;
                    }

                    // Upload the file to Azure Blob Storage.
                    var fileName = Guid.NewGuid() + fileExtension;
                    if (await _blobService.UploadFileToAzureBlob(fileName.ToString(), file))
                    {
                        comment.FileName = fileName;
                        comment.FileType = fileExtension.ToString();
                    }
                    else
                    {
                        _logger.LogWarning("Fail to upload file.");
                        return null;
                    }
                }

                // Add the comment to the database.
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return comment;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A database update error occurred while adding a comment.");
                return null;
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "A SQL error occurred while adding a comment.");
                return null;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An unexpected error occurred while adding a comment.");
                return null;
            }
        }
    }
}