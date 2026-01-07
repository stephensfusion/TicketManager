using Microsoft.EntityFrameworkCore;
using Npgsql;
using TicketManager.Data;

namespace TicketManager.DbImplementations;

/// <summary>
/// Represents the PostgreSQL implementation of the ITicket interface.
/// Provides PostgreSQL-specific methods for managing tickets and interacting with the database
/// </summary>
public class PostgreSQL<TicketStatus,Priority,Tags> : BaseDbImplementation<TicketStatus,Priority,Tags> 
where TicketStatus : struct, Enum
where Priority : struct, Enum
where Tags : struct, Enum


{
    /// <summary>
    /// Initializes a new instance of the PostgreSQL class
    /// </summary>
    /// <param name="contextFactory">The database context factory</param>
    public PostgreSQL(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory) { }

    /// <summary>
    /// Creates a PostgreSQL-specific context for migration operations.
    /// This ensures that PostgreSQL migrations are discovered and applied correctly
    /// </summary>
    /// <returns>The PostgreSQL-specific context for migrations</returns>
    protected override AppDbContext CreateMigrationContext()
    {
        // Get connection string from the base context factory
        using var baseContext = _contextFactory.CreateDbContext();
        var connectionString = baseContext.Database.GetConnectionString();

        // Create PostgreSQL-specific context for proper migration discovery
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new PostgresAppDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Gets the SQL statement for marking a migration as applied in PostgreSQL
    /// </summary>
    /// <returns>The PostgreSQL-specific SQL statement</returns>
    protected override string GetMarkMigrationSql()
    {
        return @"INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") VALUES (@migrationId, @productVersion) ON CONFLICT (""MigrationId"") DO NOTHING;";
    }

    /// <summary>
    /// Gets the parameters for marking a migration as applied in PostgreSQL
    /// </summary>
    /// <param name="migrationId">The migration ID</param>
    /// <param name="productVersion">The product version</param>
    /// <returns>The PostgreSQL-specific parameters array</returns>
    protected override object[] GetMarkMigrationParameters(string migrationId, string productVersion)
    {
        return new[]
        {
            new NpgsqlParameter("@migrationId", migrationId),
            new NpgsqlParameter("@productVersion", productVersion)
        };
    }

    /// <summary>
    /// Determines if the exception indicates a table already exists error for PostgreSQL
    /// </summary>
    /// <param name="ex">The exception to check</param>
    /// <returns>True if the exception indicates a table already exists error in PostgreSQL</returns>
    protected override bool IsTableAlreadyExistsError(Exception ex)
    {
        return ex is PostgresException pgEx && pgEx.SqlState == "42P07";
    }
}
