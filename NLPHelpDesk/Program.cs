using System.Net.Sockets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NLPHelpDesk.Data;
using NLPHelpDesk.Helpers;
using NLPHelpDesk.Interfaces;
using NLPHelpDesk.Data.Models;
using NLPHelpDesk.Services;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Connect ot DB
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Logging
builder.Services.AddLogging(logBuilder =>
{
    logBuilder.ClearProviders();
    logBuilder.AddConsole();
    logBuilder.SetMinimumLevel(LogLevel.Debug);
});

// Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    options.SlidingExpiration = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationContext>()
    .AddDefaultTokenProviders();

// DI
builder.Services.AddScoped<DatabaseHelper>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IHelpDeskCategoryService, HelpDeskCategoryService>();
builder.Services.AddScoped<IAssignUserService, AssignUserService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAzureBlobService, AzureBlobService>();
builder.Services.AddScoped<UserHelper>();
builder.Services.Configure<QueueSettings>(builder.Configuration.GetSection("AzureQueue"));
builder.Services.AddSingleton<IQueueService, QueueService>();
builder.Services.AddScoped<ICategoryPredictionService, CategoryPredictionService>();
builder.Services.AddScoped<IPriorityPredictionService, PriorityPredictionService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Policy
builder.Services.AddControllers(config =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().RequireAuthenticatedUser().Build();
    config.Filters.Add(new AuthorizeFilter(policy));
});

// Add Razor page
builder.Services.AddRazorPages();

// SignalR
//builder.Services.AddSignalR();


// Build app
var app = builder.Build();

// Run database initialization in the background
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() =>
{
    Task.Run(async () =>
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            var dbHelper = scope.ServiceProvider.GetRequiredService<DatabaseHelper>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            var retryPolicy = Policy
                .Handle<Exception>(ex =>
                    ex is SqlException sqlEx && sqlEx.Number != 0 ||
                    ex is SocketException ||
                    ex is TimeoutException ||
                    ex.Message.Contains("The connection to the server was successfully established, but then an error occurred during the login process.") ||
                    ex.Message.Contains("A network-related or instance-specific error occurred while establishing a connection to SQL Server."))
                .WaitAndRetryAsync(10,
                    retryAttempt => TimeSpan.FromSeconds(Math.Min(10 * retryAttempt, 600)),
                    onRetry: (exception, timespan, retryCount, context) =>
                    {
                        logger.LogWarning($"Database connection retry {retryCount} after {timespan}. Exception: {exception.Message}");
                    });

            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    context.Database.SetCommandTimeout(180);
                    await context.Database.MigrateAsync();
                    await dbHelper.InitializeAsync(context, roleManager);
                });

                logger.LogInformation("Database initialization completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Database initialization failed after retries: {ex.Message}");
            }
        }
    });
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();