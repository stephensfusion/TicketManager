using Microsoft.EntityFrameworkCore;
using TicketManager.Models;

namespace TicketManager.Data
{
    /// <summary>
    /// Represents the database context for user management operations.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the AppDbContext class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the schema needed for the identity framework.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Gets or sets the Tickets DbSet
        /// </summary>
        public DbSet<Ticket> Tickets { get; set; }

        /// <summary>
        /// Gets or sets the TicketMetadata DbSet
        /// </summary>
        public DbSet<TicketMetadata> TicketMetadata { get; set; }
    }

    /// <summary>
    /// PostgreSQL-specific DbContext variant for migration generation.
    /// </summary>
    public class PostgresAppDbContext : AppDbContext
    {
        /// <summary>
        /// Initializes a new instance of the PostgresAppDbContext class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public PostgresAppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }

    /// <summary>
    /// SQL Server-specific DbContext variant for migration generation.
    /// </summary>
    public class SqlServerAppDbContext : AppDbContext
    {
        /// <summary>
        /// Initializes a new instance of the SqlServerAppDbContext class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public SqlServerAppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }

    /// <summary>
    /// MySQL-specific DbContext variant for migration generation.
    /// </summary>
    public class MySQLAppDbContext : AppDbContext
    {
        /// <summary>
        /// Initializes a new instance of the MySQLAppDbContext class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public MySQLAppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}