using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLPHelpDesk.Data;
using NLPHelpDesk.Function;
using NLPHelpDesk.Function.Interfaces;
using NLPHelpDesk.Function.Services;
using NLPHelpDesk.Interfaces;
using NLPHelpDesk.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((hostContext, services) =>
    {
        // Get connection string from environment variables
        string connectionString = Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection");
        
        // Register ApplicationContext with Entity Framework Core
        services.AddDbContext<ApplicationContext>(options =>
            options.UseSqlServer(connectionString));

        // Register your existing service classes from ASP.NET Core project
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IHelpDeskCategoryService, HelpDeskCategoryService>();
        services.AddScoped<ICategoryPredictionService, CategoryPredictionService>();
        services.AddScoped<IPriorityPredictionService, PriorityPredictionService>();
        services.AddScoped<IProductService, ProductService>();
        
        services.AddScoped<TicketPredictionProcess>();

        services.AddLogging();
    })
    .Build();
    
await host.RunAsync();