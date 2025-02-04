using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NLPHelpDesk.Models;

namespace NLPHelpDesk.Contexts;

/// <summary>
/// The application's database context.
/// </summary>
public class ApplicationContext: IdentityDbContext<AppUser>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationContext"/> class.
    /// </summary>
    /// <param name="options">The options for the context.</param>
    public ApplicationContext(DbContextOptions<ApplicationContext> options): base(options){}
    
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketCompletion> TicketCompletions { get; set; }
    public DbSet<UserTicket> UserTickets { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCode> ProductCodes { get; set; }
    
    public new DbSet<AppUser> Users => base.Users;
    public DbSet<HelpDeskCategory> HelpDeskCategories { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Call the base method to configure Identity.
        base.OnModelCreating(builder);

        // Rename tables for clarity (optional, but good practice).
        builder.Entity<Ticket>().ToTable("Ticket");
        builder.Entity<TicketCompletion>().ToTable("TicketCompletion");
        builder.Entity<UserTicket>().ToTable("UserTicket");
        builder.Entity<Comment>().ToTable("Comment");
        builder.Entity<Product>().ToTable("Product");
        builder.Entity<ProductCode>().ToTable("ProductCode");
        builder.Entity<AppUser>().ToTable("AppUser");
        builder.Entity<HelpDeskCategory>().ToTable("HelpDeskCategory");
        
        // Configure relationships and constraints.
        // Ticket Configurations
        builder.Entity<Ticket>().HasKey(t => t.TicketId);
        builder.Entity<Ticket>() 
            .HasOne(t => t.HelpDeskCategory)
            .WithMany() // One-to-many relationship (HelpDeskCategory to Tickets)
            .HasForeignKey(t => t.HelpDeskCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Ticket>()
            .HasOne(t => t.Product)
            .WithMany(p => p.Tickets) // One-to-many relationship (Product to Tickets)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Ticket>()
            .HasOne(t => t.CreateUser)
            .WithMany(u => u.CreateTickets) // One-to-many relationship (AppUser to Tickets - Created By)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Ticket>()
            .HasOne(t => t.UpdateUser)
            .WithMany(ua => ua.UpdateTickets) // One-to-many relationship (AppUser to Tickets - Updated By)
            .HasForeignKey(t => t.UpdateUserId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Ticket>()
            .HasOne(t => t.TicketCompletion)
            .WithOne(tc => tc.Ticket) // One-to-one relationship (Ticket to TicketCompletion)
            .HasForeignKey<TicketCompletion>(tc => tc.TicketId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // TicketCompletion Configurations
        builder.Entity<TicketCompletion>().HasKey(t => t.CompletionId);
        builder.Entity<TicketCompletion>().Property(tc => tc.CompletionId).ValueGeneratedOnAdd(); // Auto-generate CompletionId
        builder.Entity<TicketCompletion>()
            .HasOne(tc => tc.AppUser)
            .WithMany(u => u.TicketCompletions) // One-to-many relationship (AppUser to TicketCompletions)
            .HasForeignKey(tc => tc.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<TicketCompletion>()
            .HasOne(t => t.HelpDeskCategory)
            .WithMany()
            .HasForeignKey(t => t.Category)
            .OnDelete(DeleteBehavior.SetNull);
        
        // UserTicket Configurations (Many-to-many between Users and Tickets)
        builder.Entity<UserTicket>().HasKey(ut => new { ut.UserId, ut.TicketId}); // Composite key
        builder.Entity<UserTicket>()
            .HasOne(ut => ut.AppUser)
            .WithMany(u => u.UserTickets)
            .HasForeignKey(ut => ut.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<UserTicket>()
            .HasOne(ut => ut.Ticket)
            .WithMany(t => t.UserTickets)
            .HasForeignKey(ut => ut.TicketId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // Comment Configurations
        builder.Entity<Comment>().HasKey(c => c.CommentId);
        builder.Entity<Comment>().Property(c => c.CommentId).ValueGeneratedOnAdd(); // Auto-generate CommentId
        builder.Entity<Comment>()
            .HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Comment>()
            .HasOne(c => c.AppUser)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // HelpDeskCategory Configurations
        builder.Entity<HelpDeskCategory>().HasKey(hd => hd.CategoryId);

        // AppUser Configurations
        builder.Entity<AppUser>()
            .HasOne(u => u.HelpDeskCategory)
            .WithMany() // One-to-many relationship (AppUser to HelpDeskCategories)
            .HasForeignKey(u => u.HelpDeskCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Product Configurations
        builder.Entity<Product>().HasKey(p => p.ProductId);
        builder.Entity<Product>()
            .HasOne(p => p.CreateUser)
            .WithMany(cu => cu.CreateProducts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Product>()
            .HasOne(p => p.UpdateUser)
            .WithMany(uu => uu.UpdateProducts)
            .HasForeignKey(p => p.UpdateUserId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Product>()
            .HasOne(p => p.ProductCode)
            .WithOne(p => p.Product) // One-to-one (Product to ProductCode)
            .HasForeignKey<ProductCode>(p => p.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Entity<ProductCode>().HasKey(p => p.ProductId);
    }
}