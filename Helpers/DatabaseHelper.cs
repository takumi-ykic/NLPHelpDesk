using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLPHelpDesk.Contexts;
using NLPHelpDesk.Models;
using static NLPHelpDesk.Helpers.Constants;

namespace NLPHelpDesk.Helpers;

/// <summary>
/// Provides helper methods for initializing the database with seed data.
/// </summary>
public class DatabaseHelper
{
    /// <summary>
    /// Initializes the database by applying migrations and seeding roles, help desk categories, and a default product.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="roleManager">The role manager for managing user roles.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync(ApplicationContext context, RoleManager<IdentityRole> roleManager)
    {
        bool flag = false;

        // Apply any pending database migrations.
        context.Database.Migrate();

        // Seed roles if none exist.
        if (!roleManager.Roles.Any())
        {
            await SeedRolesAsync(roleManager);
            flag = true;
        }

        // Seed help desk categories if none exist.
        if (!context.HelpDeskCategories.Any())
        {
            SeedHelpDeskCategory(context);
            flag = true;
        }

        // Seed the default product if it doesn't exist.
        if (!await context.ProductCodes.AnyAsync(p => p.ProductId == "DEFAULT"))
        {
            SeedDefaultProduct(context);
            flag = true;
        }
        
        // Save changes only if any seeding occurred. 
        if (flag)
        {
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds the help desk categories into the database.
    /// </summary>
    /// <param name="context">The application database context.</param>
    private static void SeedHelpDeskCategory(ApplicationContext context)
    {
        context.HelpDeskCategories.AddRange(
            new HelpDeskCategory { CategoryName = "Network Security" },
            new HelpDeskCategory { CategoryName = "Authentication" },
            new HelpDeskCategory { CategoryName = "Data Backup and Recovery" },
            new HelpDeskCategory { CategoryName = "Incident Response" },
            new HelpDeskCategory { CategoryName = "Malware Protection" },
            new HelpDeskCategory { CategoryName = "Mobile Security" }
            );
    }

    /// <summary>
    /// Seeds the user roles into the database.
    /// </summary>
    /// <param name="roleManager">The role manager for managing user roles.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { ROLE_ADMIN, ROLE_TECHNICIAN, ROLE_END_USER };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    /// <summary>
    /// Seeds the default product and its associated product code into the database.
    /// </summary>
    /// <param name="context">The application database context.</param>
    private static async Task SeedDefaultProduct(ApplicationContext context)
    {
        context.Products.AddRange(new Product
        {
            ProductId = "DEFAULT",
            ProductName = "DEFAULT",
            ProductDescription = "DEFAULT",
            UserId = null,
            ReleaseDate = DateTime.UtcNow,
            UpdateUserId = null,
            UpdateDate = null,
            Display = 0,
            Delete = 0
        });
        
        context.ProductCodes.AddRange( new ProductCode
            {
                ProductId = PRODUCT_CODE_DEFAULT, Code = "DEFAULT", Count = 1
            });
    }
}