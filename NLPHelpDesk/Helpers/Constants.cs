namespace NLPHelpDesk.Helpers;

/// <summary>
/// Contains constant values used throughout the application.
/// </summary>
public class Constants
{
    public const string ROLE_ADMIN = "Admin";
    public const string ROLE_TECHNICIAN = "Technician";
    public const string ROLE_END_USER = "EndUser";

    public const string VIEW_DATA_TITLE = "Title";

    public const string TEMP_DATA_ERROR_MESSAGE = "ErrorMessage";
    public const string TEMP_DATA_RETURN_URL = "ReturnUrl";

    public const string TITLE_DETAILS = "Detail: ";
    public const string TITLE_EDIT = "Edit: ";
    public const string TITLE_DELETE = "Delete: ";
    
    public const string TITLE_TICKET_INDEX = "Ticket List";
    public const string TITLE_TICKET_CREATE = "Create New Ticket";
    public const string TITLE_TICKET_COMPLETION = "Complete Ticket";
    public const string TITLE_TICKET_COMPLETION_DETAILS = "Complete Ticket Details:";
    
    public const string TITLE_PRODUCT_INDEX = "Product List";
    public const string TITLE_PRODUCT_CREATE = "Create New Product";

    public const string TITLE_ACCESS_DENIED = "Access Denied";
    
    public const string PRODUCT_CODE_DEFAULT = "DEFAULT";
    
    public const string CACHE_KEY_CATEGORY_MODEL = "CategoryModel";
    public const string CACHE_KEY_PRIORITY_MODEL = "PriorityModel";
    
    public const string AZURE_BLOB_PATH = "AzureSettings:BlobPath";
    public const string AZURE_BLOB_CONTAINER_FILES = "AzureSettings:ContainerFiles";
    public const string AZURE_STORAGE_ACCOUNT_NAME = "AzureSettings:StorageAccountName";
    public const string AZURE_BLOB_ACCESS_KEY = "AzureSettings:BlobAccessKey";
    public const string AZURE_STORAGE_QUEUE_NAME_PREDICTION = "AzureQueue:PredictionQueue";
    public const string AZURE_WEB_JOBS_STORAGE = "AzureWebJobsStorage";

    public const int MAX_FILE_SIZE = 8 * 1024 * 1024;
    public static readonly string[] ALLOWED_FILE_EXTENSION = { ".jpeg", ".jpg", ".png", ".zip", ".txt", ".pdf" };
}