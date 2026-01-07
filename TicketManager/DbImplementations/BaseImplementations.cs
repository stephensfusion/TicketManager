using System.Linq.Expressions;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NSCore.DatabaseContext;
using NSCore.SearchOption;
using NSCore.Validation;
using TicketManager.Data;
using TicketManager.DTOs;
using TicketManager.Enumerations;
using TicketManager.Interfaces;
using TicketManager.Models;

namespace TicketManager.DbImplementations;

/// <summary>
/// Base implementation class for ticket management operations.
/// Contains common logic shared across all database providers.
/// </summary>
public abstract class BaseDbImplementation<TicketStatus, Priority, Tags> : INsContextInit, ITicketManager<TicketStatus, Priority, Tags>, IDisposable
where TicketStatus : struct, Enum
where Priority : struct, Enum
where Tags : struct, Enum


{
    protected bool _IsDisposed = false;
    protected readonly IDbContextFactory<AppDbContext> _contextFactory;

    #region Compiled Queries

    //Compiled queries for frequently used executed  operations
    protected static readonly Func<AppDbContext, int, Task<Ticket?>> GetTicketByIdQuery =
       EF.CompileAsyncQuery((AppDbContext context, int ticketId) =>
       context.Tickets.AsNoTracking().Where(t => t.TicketId == ticketId).Select(x => new Ticket
       {
           TicketId = x.TicketId,
           AssigneeEmail = x.AssigneeEmail,
           Description = x.Description,
           PromiseDate = x.PromiseDate,
           TicketStatus = x.TicketStatus,
           Title = x.Title,
           Attachments = x.Attachments,
           CreatorEmail = x.CreatorEmail,
           Priority = x.Priority,
           Tag = x.Tag,
           AdditionalInfo = x.AdditionalInfo,
           CreatedDate = x.CreatedDate,
           UpdatedDate = x.UpdatedDate,
           CCS = x.CCS,
           Metadata = new List<TicketMetadata>()
       }).FirstOrDefault());

    protected static readonly Func<AppDbContext, int, Task<Ticket?>> GetTicketByIdWithMetadataQuery =
           EF.CompileAsyncQuery((AppDbContext context, int ticketId) =>
           context.Tickets.AsNoTracking().Include(x => x.Metadata).Where(u => u.TicketId == ticketId).Select(x => new Ticket
           {
               TicketId = x.TicketId,
               AssigneeEmail = x.AssigneeEmail,
               Description = x.Description,
               PromiseDate = x.PromiseDate,
               TicketStatus = x.TicketStatus,
               Title = x.Title,
               Attachments = x.Attachments,
               CreatorEmail = x.CreatorEmail,
               Priority = x.Priority,
               Tag = x.Tag,
               AdditionalInfo = x.AdditionalInfo,
               CreatedDate = x.CreatedDate,
               UpdatedDate = x.UpdatedDate,
               CCS = x.CCS,
               Metadata = x.Metadata != null ? x.Metadata.Select(m => new TicketMetadata
               {
                   Id = m.Id,
                   TicketId = m.TicketId,
                   Key = m.Key,
                   Value = m.Value
               }).ToList() : new List<TicketMetadata>()
           }).FirstOrDefault());

    protected static readonly Func<AppDbContext, string, Task<Ticket?>> GetTicketByTitleQuery =
     EF.CompileAsyncQuery((AppDbContext context, string title) =>
     context.Tickets.AsNoTracking().Where(u => u.Title == title).Select(x => new Ticket
     {
         TicketId = x.TicketId,
         AssigneeEmail = x.AssigneeEmail,
         Description = x.Description,
         PromiseDate = x.PromiseDate,
         TicketStatus = x.TicketStatus,
         Title = x.Title,
         Attachments = x.Attachments,
         CreatorEmail = x.CreatorEmail,
         Priority = x.Priority,
         Tag = x.Tag,
         AdditionalInfo = x.AdditionalInfo,
         CreatedDate = x.CreatedDate,
         UpdatedDate = x.UpdatedDate,
         CCS = x.CCS,
         Metadata = new List<TicketMetadata>()
     }).FirstOrDefault());

    protected static readonly Func<AppDbContext, string, Task<Ticket?>> GetTicketByTitleWithMetadataQuery =
           EF.CompileAsyncQuery((AppDbContext context, string title) =>
           context.Tickets.AsNoTracking().Include(x => x.Metadata).Where(u => u.Title == title).Select(x => new Ticket
           {
               TicketId = x.TicketId,
               AssigneeEmail = x.AssigneeEmail,
               Description = x.Description,
               PromiseDate = x.PromiseDate,
               TicketStatus = x.TicketStatus,
               Title = x.Title,
               Attachments = x.Attachments,
               CreatorEmail = x.CreatorEmail,
               Priority = x.Priority,
               Tag = x.Tag,
               AdditionalInfo = x.AdditionalInfo,
               CreatedDate = x.CreatedDate,
               UpdatedDate = x.UpdatedDate,
               CCS = x.CCS,
               Metadata = x.Metadata != null ? x.Metadata.Select(m => new TicketMetadata
               {
                   Id = m.Id,
                   TicketId = m.TicketId,
                   Key = m.Key,
                   Value = m.Value
               }).ToList() : new List<TicketMetadata>()
           }).FirstOrDefault());

    protected static readonly Func<AppDbContext, string, Task<Ticket?>> GetTicketByAssigneeQuery =
          EF.CompileAsyncQuery((AppDbContext context, string assignee) =>
          context.Tickets.AsNoTracking().Where(u => u.AssigneeEmail == assignee).Select(x => new Ticket
          {
              TicketId = x.TicketId,
              AssigneeEmail = x.AssigneeEmail,
              Description = x.Description,
              PromiseDate = x.PromiseDate,
              TicketStatus = x.TicketStatus,
              Title = x.Title,
              Attachments = x.Attachments,
              CreatorEmail = x.CreatorEmail,
              Priority = x.Priority,
              Tag = x.Tag,
              AdditionalInfo = x.AdditionalInfo,
              CreatedDate = x.CreatedDate,
              UpdatedDate = x.UpdatedDate,
              CCS = x.CCS,
              Metadata = new List<TicketMetadata>()
          }).FirstOrDefault());

    protected static readonly Func<AppDbContext, string, Task<Ticket?>> GetTicketByAssigneeWithMetadataQuery =
           EF.CompileAsyncQuery((AppDbContext context, string assignee) =>
           context.Tickets.AsNoTracking().Include(x => x.Metadata).Where(u => u.AssigneeEmail == assignee).Select(x => new Ticket
           {
               TicketId = x.TicketId,
               AssigneeEmail = x.AssigneeEmail,
               Description = x.Description,
               PromiseDate = x.PromiseDate,
               TicketStatus = x.TicketStatus,
               Title = x.Title,
               Attachments = x.Attachments,
               CreatorEmail = x.CreatorEmail,
               Priority = x.Priority,
               Tag = x.Tag,
               AdditionalInfo = x.AdditionalInfo,
               CreatedDate = x.CreatedDate,
               UpdatedDate = x.UpdatedDate,
               CCS = x.CCS,
               Metadata = x.Metadata != null ? x.Metadata.Select(m => new TicketMetadata
               {
                   Id = m.Id,
                   TicketId = m.TicketId,
                   Key = m.Key,
                   Value = m.Value
               }).ToList() : new List<TicketMetadata>()
           }).FirstOrDefault());

    protected static readonly Func<AppDbContext, string, Task<Ticket?>> GetTicketByStatusQuery =
          EF.CompileAsyncQuery((AppDbContext context, string ticketStatus) =>
          context.Tickets.AsNoTracking().Where(u => u.TicketStatus == ticketStatus).Select(x => new Ticket
          {
              TicketId = x.TicketId,
              AssigneeEmail = x.AssigneeEmail,
              Description = x.Description,
              PromiseDate = x.PromiseDate,
              TicketStatus = x.TicketStatus,
              Title = x.Title,
              Attachments = x.Attachments,
              CreatorEmail = x.CreatorEmail,
              Priority = x.Priority,
              Tag = x.Tag,
              AdditionalInfo = x.AdditionalInfo,
              CreatedDate = x.CreatedDate,
              UpdatedDate = x.UpdatedDate,
              CCS = x.CCS,
              Metadata = new List<TicketMetadata>()
          }).FirstOrDefault());

    protected static readonly Func<AppDbContext, string, Task<Ticket?>> GetTicketByStatusWithMetadataQuery =
           EF.CompileAsyncQuery((AppDbContext context, string ticketStatus) =>
           context.Tickets.AsNoTracking().Include(x => x.Metadata).Where(u => u.TicketStatus == ticketStatus).Select(x => new Ticket
           {
               TicketId = x.TicketId,
               AssigneeEmail = x.AssigneeEmail,
               Description = x.Description,
               PromiseDate = x.PromiseDate,
               TicketStatus = x.TicketStatus,
               Title = x.Title,
               Attachments = x.Attachments,
               CreatorEmail = x.CreatorEmail,
               Priority = x.Priority,
               Tag = x.Tag,
               AdditionalInfo = x.AdditionalInfo,
               CreatedDate = x.CreatedDate,
               UpdatedDate = x.UpdatedDate,
               CCS = x.CCS,
               Metadata = x.Metadata != null ? x.Metadata.Select(m => new TicketMetadata
               {
                   Id = m.Id,
                   TicketId = m.TicketId,
                   Key = m.Key,
                   Value = m.Value
               }).ToList() : new List<TicketMetadata>()
           }).FirstOrDefault());


    protected static readonly Func<AppDbContext, Task<int>> GetTotalTicketCountQuery =
        EF.CompileAsyncQuery((AppDbContext context) =>
            context.Tickets.Count());
    protected static readonly Func<AppDbContext, int, Task<bool>> TicketExistsByIdQuery =
        EF.CompileAsyncQuery((AppDbContext context, int ticketId) =>
        context.Tickets.Any(s => s.TicketId == ticketId));

    protected static readonly Func<AppDbContext, string, Task<bool>> TicketTitleExistsQuery =
        EF.CompileAsyncQuery((AppDbContext context, string title) =>
        context.Tickets.Any(u => u.Title == title));


    protected static readonly Func<AppDbContext, string, Task<bool>> TicketExistsByAssigneeEmailQuery =
        EF.CompileAsyncQuery((AppDbContext context, string assignee) =>
        context.Tickets.Any(u => u.AssigneeEmail == assignee));

    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the BaseDbImplementation class.
    /// </summary>
    /// <param name="contextFactory">The database context factory</param>
    /// <exception cref="ArgumentNullException">Throws an argument null exception</exception>
    protected BaseDbImplementation(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        InitializeConnection();
    }

    /// <summary>
    /// Initializes the database connection. Can be overridden by derived classes.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws an invalid operation exception</exception>
    protected virtual void InitializeConnection()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            if (!context.Database.CanConnect())
            {
                throw new InvalidOperationException("Unable to connect to the database. Please check the connection string and database server.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize the database context.", ex);
        }
    }
    #endregion

    #region Helper Methods Using Complied Queries
    /// <summary>
    /// Checks if the ticket exists by title using compiled queries
    /// </summary>
    /// <param name="title">The title to check</param>
    /// <returns>True if the title exist, false if not</returns>
    protected async Task<bool> TicketExistsByTitleAsync(string title)
    {
        using var context = _contextFactory.CreateDbContext();
        return await TicketTitleExistsQuery(context, title);
    }

    /// <summary>
    /// Checks if the ticket exists by ID using compiled queries
    /// </summary>
    /// <param name="ticketId">The ID to check</param>
    /// <returns>True if ID exists, else false</returns>
    protected async Task<bool> TicketExistsByIdAsync(int ticketId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await TicketExistsByIdQuery(context, ticketId);
    }

    /// <summary>
    /// Checks if ticket exists by user name using compiled queries
    /// </summary>
    /// <param name="assignee">User name to check</param>
    /// <returns>True if name exists,else false</returns>
    protected async Task<bool> TicketExistsByAssigneeEmailAsync(string name)
    {
        using var context = _contextFactory.CreateDbContext();
        return await TicketExistsByAssigneeEmailQuery(context, name);
    }
    #endregion

    #region Helper Methods for Enum Parsing
    protected virtual TicketStatus ParseTicketStatusOrDefault<TicketStatus>(object? value, Func<TicketStatus> getDefaultValue) where TicketStatus : struct, Enum
    {
        if (value == null) return getDefaultValue();

        if (value is TicketStatus enumValue) return enumValue;

        if (value is string stringValue && Enum.TryParse<TicketStatus>(stringValue, true, out TicketStatus result))
            return result;

        return getDefaultValue();
    }

    protected virtual TicketStatus GetFirstTicketStatusEnumValue<TicketStatus>() where TicketStatus : Enum
    {
        var enumValues = Enum.GetValues(typeof(TicketStatus));
        if (enumValues.Length == 0)
        {
            throw new InvalidOperationException($"Enum type {typeof(TicketStatus).Name} has no values defined.");
        }

        var firstValue = enumValues.GetValue(0);
        if (firstValue == null)
        {
            throw new InvalidOperationException("The first value of the Status enum is null.");
        }
        return (TicketStatus)firstValue;
    }

    protected virtual Priority ParseTicketPriorityOrDefault<Priority>(object? value, Func<Priority> getDefaultValue) where Priority : struct, Enum
    {
        if (value == null) return getDefaultValue();

        if (value is Priority enumValue) return enumValue;

        if (value is string stringValue && Enum.TryParse<Priority>(stringValue, true, out Priority result))
            return result;

        return getDefaultValue();
    }


    protected virtual Priority GetFirstTicketPriorityEnumValue<Priority>() where Priority : Enum
    {
        var enumValues = Enum.GetValues(typeof(Priority));
        if (enumValues.Length == 0)
        {
            throw new InvalidOperationException($"Enum type {typeof(Priority).Name} has no values defined.");
        }

        var firstValue = enumValues.GetValue(0);
        if (firstValue == null)
        {
            throw new InvalidOperationException("The first value of the priority enum is null.");
        }
        return (Priority)firstValue;
    }

    protected virtual Tag ParseTicketTagOrDefault<Tag>(object? value, Func<Tag> getDefaultValue) where Tag : struct, Enum
    {
        if (value == null) return getDefaultValue();

        if (value is Tag enumValue) return enumValue;

        if (value is string stringValue && Enum.TryParse<Tag>(stringValue, true, out Tag result))
            return result;

        return getDefaultValue();
    }

    protected virtual Tag GetFirstTicketTagEnumValue<Tag>() where Tag : Enum
    {
        var enumValues = Enum.GetValues(typeof(Tag));
        if (enumValues.Length == 0)
        {
            throw new InvalidOperationException($"Enum type {typeof(Tag).Name} has no values defined.");
        }

        var firstValue = enumValues.GetValue(0);
        if (firstValue == null)
        {
            throw new InvalidOperationException("The first value of the tag enum is null.");
        }
        return (Tag)firstValue;
    }
    #endregion

    #region Migration Methods
    /// <summary>
    /// Creates a database context for migration operations.
    /// Can be overridden by derived classes to provide database-specific contexts.
    /// </summary>
    /// <returns>The database context to use for migrations</returns>
    protected virtual AppDbContext CreateMigrationContext()
    {
        return _contextFactory.CreateDbContext();
    }

    /// <summary>
    /// Applies migrations to the database asynchronously.
    /// This method uses the database-specific context for proper migration discovery.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="Exception">Handles any exceptions occurred</exception>
    public virtual async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var context = CreateMigrationContext();
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                throw new InvalidOperationException("Database doesn't exist. Please create and configure a database first.");
            }

            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync(cancellationToken);
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);

            if (pendingMigrations.Any())
            {
                await context.Database.MigrateAsync(cancellationToken);
            }
        }
        catch (Exception ex) when (IsTableAlreadyExistsError(ex))
        {
            await HandleMigrationConflictAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Migration failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Determines if the exception indicates a table already exists error.
    /// Can be overridden by derived classes for database-specific error handling.
    /// </summary>
    /// <param name="ex">The exception to check</param>
    /// <returns>True if the exception indicates a table already exists error</returns>
    protected virtual bool IsTableAlreadyExistsError(Exception ex)
    {
        // Base implementation - override in derived classes for specific database error codes
        return false;
    }

    /// <summary>
    /// Marks a migration as applied in the database.
    /// Can be overridden by derived classes for database-specific implementation.
    /// </summary>
    /// <param name="migrationId">The migration ID to mark as applied</param>
    /// <param name="cancellationToken">The cancellation token</param>
    protected virtual async Task MarkMigrationAsAppliedAsync(string migrationId, CancellationToken cancellationToken)
    {
        using var context = _contextFactory.CreateDbContext();
        var provider = context.Database.ProviderName;
        var productVersion = typeof(DbContext).Assembly.GetName().Version?.ToString() ?? "8.0.0";

        var rawQuery = GetMarkMigrationSql();
        var parameters = GetMarkMigrationParameters(migrationId, productVersion);

        await context.Database.ExecuteSqlRawAsync(rawQuery, parameters, cancellationToken);
    }

    /// <summary>
    /// Gets the SQL statement for marking a migration as applied.
    /// Must be overridden by derived classes.
    /// </summary>
    /// <returns>The SQL statement</returns>
    protected abstract string GetMarkMigrationSql();

    /// <summary>
    /// Gets the parameters for marking a migration as applied.
    /// Must be overridden by derived classes.
    /// </summary>
    /// <param name="migrationId">The migration ID</param>
    /// <param name="productVersion">The product version</param>
    /// <returns>The parameters array</returns>
    protected abstract object[] GetMarkMigrationParameters(string migrationId, string productVersion);

    private async Task HandleMigrationConflictAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            var allMigrations = context.Database.GetMigrations().ToList();
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            var latestMigration = allMigrations.LastOrDefault();

            if (latestMigration != null && pendingMigrations.Contains(latestMigration))
            {
                foreach (var migration in pendingMigrations)
                {
                    await MarkMigrationAsAppliedAsync(migration, cancellationToken);
                }
            }
            else
            {
                throw new InvalidOperationException("Could not resolve conflict: No valid latest migration found.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to resolve migration conflict: {ex.Message}", ex);
        }
    }
    #endregion

    /// <inheritdoc/>
    public virtual bool IsContextCreated()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return context.Database.CanConnect();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to check if the context is created.", ex);
        }
    }
    #region Create ticket
    /// <inheritdoc/>
    public async Task<Ticket> CreateTicketAsync(CreateTicketDTO createTicketDto, List<IFormFile> files)
    {
        if (createTicketDto.Metadata != null && createTicketDto.Metadata.Any(x => string.IsNullOrWhiteSpace(x.Key) || string.IsNullOrWhiteSpace(x.Value)))
        {
            throw new ArgumentException("Metadata entries must have non-empty keys and values.", nameof(createTicketDto.Metadata));
        }
        //Check for existing tickets using compiled queries 
        if (await TicketExistsByTitleAsync(createTicketDto.Title!))
        {
            throw new InvalidOperationException($"Ticket with title {createTicketDto.Title} already exists");
        }
        if (await TicketExistsByAssigneeEmailAsync(createTicketDto.CreatorEmail))
        {
            throw new InvalidOperationException($"Ticket with this user email,{createTicketDto.CreatorEmail} already exists");
        }

        using var context = _contextFactory.CreateDbContext();
        try
        {
            // Parse or default the status
            if (!string.IsNullOrWhiteSpace(createTicketDto.TicketStatus))
            {
                InputValidator.Validate<TicketStatus>(createTicketDto.TicketStatus);
                var statusEnum = Enum.Parse<TicketStatus>(createTicketDto.TicketStatus, ignoreCase: true);
                createTicketDto.TicketStatus = statusEnum.ToString();
            }
            else
            {
                createTicketDto.TicketStatus = GetFirstTicketStatusEnumValue<TicketStatus>().ToString();
            }
            // Parse or default the priority
            if (!string.IsNullOrWhiteSpace(createTicketDto.Priority))
            {
                InputValidator.Validate<Priority>(createTicketDto.Priority);
                var priorityEnum = Enum.Parse<Priority>(createTicketDto.Priority, ignoreCase: true);
                createTicketDto.Priority = priorityEnum.ToString();
            }
            else
            {
                createTicketDto.Priority = GetFirstTicketPriorityEnumValue<Priority>().ToString();
            }
            // Parse or default the tag
            if (!string.IsNullOrWhiteSpace(createTicketDto.Tag))
            {
                InputValidator.Validate<Tags>(createTicketDto.Tag);
                var tagEnum = Enum.Parse<Tags>(createTicketDto.Tag, ignoreCase: true);
                createTicketDto.Tag = tagEnum.ToString();
            }
            else
            {
                createTicketDto.Tag = GetFirstTicketTagEnumValue<Tags>().ToString();
            }

            var ticket = new Ticket
            {
                AssigneeEmail = createTicketDto.AssigneeEmail ?? null,
                CreatorEmail = createTicketDto.CreatorEmail,
                PromiseDate = createTicketDto.PromiseDate,
                Description = createTicketDto.Description,
                Title = createTicketDto.Title,
                TicketStatus = ParseTicketStatusOrDefault<TicketStatus>(createTicketDto.TicketStatus, GetFirstTicketStatusEnumValue<TicketStatus>).ToString(),
                Priority = ParseTicketPriorityOrDefault<Priority>(createTicketDto.Priority, GetFirstTicketPriorityEnumValue<Priority>).ToString(),
                Tag = ParseTicketTagOrDefault<Tags>(createTicketDto.Tag, GetFirstTicketTagEnumValue<Tags>).ToString(),
                CreatedDate = DateTimeOffset.UtcNow,
                UpdatedDate = DateTimeOffset.UtcNow,
                AdditionalInfo = createTicketDto.AdditionalInfo,
                CCS = createTicketDto.CCS ?? new List<string>()
            };

            await context.Tickets.AddAsync(ticket);
            await context.SaveChangesAsync();

            int max = 2;
            if (files.Count > max) throw new Exception
            ("Maximum of 2 files accepted");

            //Check if file contents are the same for 2 files upload
            if (files.Count == 2)
            {
                if (!FilesAreEqual(files[0], files[1]))
                {
                    throw new Exception("Files chosen for upload have the name");
                }
                else
                {
                    var fileNames = await UploadFilesAsync(files, ticket.TicketId);
                    ticket.Attachments.AddRange(fileNames);
                    await context.SaveChangesAsync();
                }
            }
            else
            {
                var fileNames = await UploadFilesAsync(files, ticket.TicketId);
                ticket.Attachments.AddRange(fileNames);
                await context.SaveChangesAsync();
            }
            return ticket;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Failed to create ticket/Ticket created without attachment", exception);
        }
    }
    #endregion

    #region Check if files are equal
    private bool FilesAreEqual(IFormFile first, IFormFile second)
    {
        if (first == second) return true;
        if (first.Length != second.Length) return false;

        const int bufferSize = 4096;
        var buffer1 = new byte[bufferSize];
        var buffer2 = new byte[bufferSize];

        using (var stream1 = first.OpenReadStream())
        using (var stream2 = second.OpenReadStream())
        {
            int read1, read2;
            while ((read1 = stream1.Read(buffer1, 0, bufferSize)) > 0)
            {
                read2 = stream2.Read(buffer2, 0, bufferSize);
                if (read1 != read2) return false;

                if (!buffer1.AsSpan(0, read1).SequenceEqual(buffer2.AsSpan(0, read2)))
                    return false;
            }

            return stream2.Read(buffer2, 0, 1) == 0;
        }
    }
    #endregion

    #region Delete file
    private static async Task DeleteFileWithRetryAsync(string filePath, int maxRetries, int delayMs)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                return;
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                await Task.Delay(delayMs);
            }
            catch (UnauthorizedAccessException) when (i < maxRetries - 1)
            {
                await Task.Delay(delayMs);
            }
        }

        try
        {
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            throw new IOException($"Unable to delete file after {maxRetries} attempts: {filePath}", ex);
        }
    }
    #endregion

    #region Delete directory
    private static async Task DeleteDirectoryWithRetryAsync(string directoryPath, int maxRetries, int delayMs)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, recursive: true);
                }
                return;
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                await Task.Delay(delayMs);
            }
            catch (UnauthorizedAccessException) when (i < maxRetries - 1)
            {
                await Task.Delay(delayMs);
            }
        }

        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    #endregion

    #region Upload file 
    /// <inheritdoc/>
    public async Task<List<string>> UploadFilesAsync(List<IFormFile> formFiles, int ticketId)
    {
        try
        {
            List<string> fileNames = new();

            DirectoryInfo baseDirectory = new DirectoryInfo("UploadedFiles");

            if (!baseDirectory.Exists)
            {
                baseDirectory.Create();
            }
            DirectoryInfo subDirectory = baseDirectory.CreateSubdirectory($"Ticket No.{ticketId}");

            foreach (var file in formFiles)
            {
                string filePath = Path.Combine("UploadedFiles", $"Ticket No.{ticketId}", file.FileName);
                FileStream fileStream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(fileStream);
                fileNames.Add(filePath);
            }
            return fileNames;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while uploading file", ex);
        }
    }
    #endregion

    #region Get ticket by ID
    /// <inheritdoc/>
    public async Task<Ticket> GetTicketByIdAsync(int ticketId, bool includeMetadata = false)
    {
        if (ticketId <= 0)
            throw new ArgumentException("Ticket ID must be greater than zero.");

        try
        {
            var context = _contextFactory.CreateDbContext();

            if (includeMetadata)
            {
                return await GetTicketByIdQuery(context, ticketId);
            }
            else
            {
                return await GetTicketByIdQuery(context, ticketId);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to get ticket by ID", ex);
        }
    }
    #endregion

    #region Get all tickets
    /// <inheritdoc/>
    public async Task<List<Ticket>> GetAllTicketsAsync(int pageNumber = 1, int pageSize = 10, bool includeMetadata = false)
    {
        if (pageNumber <= 0)
            throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));

        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

        try
        {
            using var context = _contextFactory.CreateDbContext();

            var totalCount = await GetTotalTicketCountQuery(context);
            var skip = (pageNumber - 1) * pageSize;

            var query = context.Tickets.AsNoTracking();
            if (includeMetadata)
            {
                query = query.Include(u => u.Metadata);
            }

            var tickets = await query
                .OrderBy(u => u.TicketId)
                .Skip(skip)
                .Take(pageSize)
                .Select(u => new Ticket
                {
                    TicketId = u.TicketId,
                    AssigneeEmail = u.AssigneeEmail,
                    Description = u.Description,
                    PromiseDate = u.PromiseDate,
                    TicketStatus = u.TicketStatus,
                    Title = u.Title,
                    Attachments = u.Attachments,
                    CreatorEmail = u.CreatorEmail,
                    Priority = u.Priority,
                    Tag = u.Tag,
                    AdditionalInfo = u.AdditionalInfo,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                    CCS = u.CCS,
                    Metadata = includeMetadata && u.Metadata != null
                        ? u.Metadata.Select(m => new TicketMetadata
                        {
                            Id = m.Id,
                            TicketId = m.TicketId,
                            Key = m.Key,
                            Value = m.Value
                        }).ToList()
                        : new List<TicketMetadata>()
                })
                .ToListAsync();
            return tickets;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving all tickets.", ex);
        }
    }
    #endregion

    #region Update Ticket
    /// <inheritdoc/>
    public async Task<Ticket> UpdateTicketAsync(int ticketId, UpdateTicketDTO updateTicketDTO)
    {
        if (ticketId <= 0)
            throw new ArgumentException("Ticket ID must be greater than zero.");

        if (updateTicketDTO == null)
            throw new ArgumentNullException(nameof(updateTicketDTO));

        using var context = _contextFactory.CreateDbContext();
        var existingTicket = await GetTicketByIdQuery(context, ticketId);

        if (existingTicket == null)
            throw new InvalidOperationException($"Ticket with ID {ticketId} not found.");

        if (!string.IsNullOrWhiteSpace(updateTicketDTO.Title) && updateTicketDTO.Title != existingTicket.Title)
        {
            if (await TicketExistsByTitleAsync(updateTicketDTO.Title))
                throw new InvalidOperationException($"Ticket with title '{updateTicketDTO.Title}' already exists.");

            existingTicket.Title = updateTicketDTO.Title;
        }

        if (!string.IsNullOrWhiteSpace(updateTicketDTO.AssigneeEmail))
            existingTicket.AssigneeEmail = updateTicketDTO.AssigneeEmail;

        if (!string.IsNullOrWhiteSpace(updateTicketDTO.AdditionalInfo))
            existingTicket.AdditionalInfo = updateTicketDTO.AdditionalInfo;

        if (!string.IsNullOrWhiteSpace(updateTicketDTO.Description))
            existingTicket.Description = updateTicketDTO.Description;

        if (updateTicketDTO.PromiseDate.HasValue)
            existingTicket.PromiseDate = updateTicketDTO.PromiseDate;

        try
        {
            if (updateTicketDTO.CCS != null)
            {
                existingTicket.CCS.Clear();
                var validCcs = updateTicketDTO.CCS
                    .Where(cc => !string.IsNullOrWhiteSpace(cc))
                    .ToList();
                existingTicket.CCS.AddRange(validCcs);
            }

            if (!string.IsNullOrWhiteSpace(updateTicketDTO.TicketStatus))
                existingTicket.TicketStatus = Enum.Parse<TicketStatus>(updateTicketDTO.TicketStatus, ignoreCase: true).ToString();

            if (!string.IsNullOrWhiteSpace(updateTicketDTO.Priority))
                existingTicket.Priority = Enum.Parse<Priority>(updateTicketDTO.Priority, ignoreCase: true).ToString();

            if (!string.IsNullOrWhiteSpace(updateTicketDTO.Tag))
                existingTicket.Tag = Enum.Parse<Tags>(updateTicketDTO.Tag, ignoreCase: true).ToString();

            // Updating metadata if provided
            if (updateTicketDTO.Metadata != null)
            {
                // Remove existing metadata
                if (existingTicket.Metadata?.Any() == true)
                {
                    context.TicketMetadata.RemoveRange(existingTicket.Metadata);
                }
                // Add new metadata
                var newMetadata = updateTicketDTO.Metadata.Select(kvp => new TicketMetadata
                {
                    TicketId = ticketId,
                    Ticket = existingTicket,
                    Key = kvp.Key,
                    Value = kvp.Value
                }).ToList();
                context.TicketMetadata.AddRange(newMetadata);
            }

            existingTicket.UpdatedDate = DateTimeOffset.UtcNow;
            context.Update(existingTicket);
            await context.SaveChangesAsync();
            return existingTicket;
        }
        catch (Exception)
        {
            throw new InvalidOperationException("Failed to update ticket");
        }
    }
    #endregion

    #region Delete Ticket Attachment
    /// <inheritdoc/>
    public async Task DeleteAttachmentAsync(int ticketId, string nameOfFile)
    {
        if (ticketId <= 0)
            throw new ArgumentException("Ticket ID must be greater than zero.");

        if (File.Exists($"UploadedFiles{nameOfFile}"))
            File.Delete($"UploadedFiles{nameOfFile}");

        try
        {
            using var context = _contextFactory.CreateDbContext();
            var ticket = await GetTicketByIdWithMetadataQuery(context, ticketId);

            if (ticket == null)
                throw new InvalidOperationException($"Ticket with ID {ticketId} not found.");

            if (ticket.Metadata?.Any() == true)
            {
                context.TicketMetadata.RemoveRange(ticket.Metadata);
            }
            string fileName = Path.Combine("UploadedFiles", $"Ticket No.{ticketId}", nameOfFile);

            if (ticket.Attachments.Contains(fileName))
            {
                File.Delete(fileName);
                ticket.Attachments.Remove(fileName);
                context.Tickets.Update(ticket);
                await context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Could not find ticket attachment");
            }
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Failed to delete ticket attachment", exception);
        }
    }
    #endregion

    #region Delete ticket
    /// <inheritdoc/>
    public async Task DeleteTicketAsync(int ticketId)
    {
        if (ticketId <= 0)
            throw new ArgumentException("Ticket ID must be greater than zero.");

        await using var context = _contextFactory.CreateDbContext();
        var ticket = await context.Tickets.Include(u => u.Metadata).FirstOrDefaultAsync(u => u.TicketId == ticketId);

        if (ticket == null)
            throw new InvalidOperationException($"Ticket with ID {ticketId} not found.");

        try
        {
            // Removimg ticket metadata first
            if (ticket.Metadata?.Any() == true)
            {
                context.TicketMetadata.RemoveRange(ticket.Metadata);
            }

            var ticketDirectory = Path.Combine("UploadedFiles", $"Ticket No.{ticketId}");

            if (Directory.Exists(ticketDirectory))
            {
                if (ticket.Attachments != null)
                {
                    foreach (var filePath in ticket.Attachments)
                    {
                        if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                        {
                            await DeleteFileWithRetryAsync(filePath, 5, 500);
                        }
                    }
                }
                await DeleteDirectoryWithRetryAsync(ticketDirectory, maxRetries: 5, delayMs: 5);
            }

            // Remove the ticket
            context.Tickets.Remove(ticket);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while deleting ticket.", ex);
        }
    }
    #endregion

    #region Fetch Ticket
    /// <inheritdoc/>
    public async Task<List<Ticket>> FetchTicketsAsync(int from = 0, int pageSize = 10, bool includeMetadata = false)
    {
        if (from < 0)
            throw new ArgumentException("From index cannot be negative.", nameof(from));

        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

        try
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Tickets.AsNoTracking();

            if (includeMetadata)
            {
                query = query.Include(u => u.Metadata);
            }
            var tickets = await query
                .OrderBy(u => u.TicketId)
                .Skip(from)
                .Take(pageSize)
                .Select(u => new Ticket
                {
                    TicketId = u.TicketId,
                    AssigneeEmail = u.AssigneeEmail,
                    Description = u.Description,
                    PromiseDate = u.PromiseDate,
                    TicketStatus = u.TicketStatus,
                    Title = u.Title,
                    Attachments = u.Attachments,
                    CreatorEmail = u.CreatorEmail,
                    Priority = u.Priority,
                    Tag = u.Tag,
                    AdditionalInfo = u.AdditionalInfo,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                    CCS = u.CCS,
                    Metadata = includeMetadata && u.Metadata != null
                        ? u.Metadata.Select(m => new TicketMetadata
                        {
                            Id = m.Id,
                            TicketId = m.TicketId,
                            Key = m.Key,
                            Value = m.Value
                        }).ToList()
                        : new List<TicketMetadata>()
                })
                .ToListAsync();
            return tickets;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while fetching tickets", ex);
        }
    }
    #endregion

    #region Search Tickets
    /// <inheritdoc/>
    public async Task<(IEnumerable<Ticket> Tickets, int TotalCount)> SearchTicketsAsync(
        string? searchTerm = null,
        string? status = null,
        string? tag = null,
        string? priority = null,
        string sortBy = "Name",
        bool ascending = true,
        int pageNumber = 1,
        int pageSize = 10,
        SearchMode searchMode = SearchMode.Contains,
        bool includeMetadata = false)
    {
        if (pageNumber <= 0)
            throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));

        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

        try
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Tickets.AsNoTracking();

            if (includeMetadata)
            {
                query = query.Include(u => u.Metadata);
            }

            // Generic search
            query = query.ApplySearch(searchTerm, searchMode, u => u.CreatorEmail, u => u.Title, u => u.AssigneeEmail, u => u.Description);

            // Optional filters
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, true, out TicketStatus parsedStatus))
            {
                query = query.Where(u => u.TicketStatus == parsedStatus.ToString());
            }
            if (!string.IsNullOrWhiteSpace(tag) && Enum.TryParse<Tags>(tag, true, out Tags parsedTags))
            {
                query = query.Where(u => u.Tag == parsedTags.ToString());
            }
            if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TicketStatus>(priority, true, out TicketStatus parsedPriority))
            {
                query = query.Where(u => u.Priority == parsedPriority.ToString());
            }

            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "name" => ascending ? query.OrderBy(u => u.CreatorEmail) : query.OrderByDescending(u => u.CreatorEmail),
                "title" => ascending ? query.OrderBy(u => u.Title) : query.OrderByDescending(u => u.Title),
                "assignee" => ascending ? query.OrderBy(u => u.AssigneeEmail) : query.OrderByDescending(u => u.AssigneeEmail),
                "createdat" => ascending ? query.OrderBy(u => u.CreatedDate) : query.OrderByDescending(u => u.CreatedDate),
                "updatedat" => ascending ? query.OrderBy(u => u.UpdatedDate) : query.OrderByDescending(u => u.UpdatedDate),
                _ => ascending ? query.OrderBy(u => u.CreatorEmail) : query.OrderByDescending(u => u.CreatorEmail)
            };

            // Apply pagination
            var skip = (pageNumber - 1) * pageSize;
            var tickets = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(u => new Ticket
                {
                    TicketId = u.TicketId,
                    AssigneeEmail = u.AssigneeEmail,
                    Description = u.Description,
                    PromiseDate = u.PromiseDate,
                    TicketStatus = u.TicketStatus,
                    Title = u.Title,
                    Attachments = u.Attachments,
                    CreatorEmail = u.CreatorEmail,
                    Priority = u.Priority,
                    Tag = u.Tag,
                    AdditionalInfo = u.AdditionalInfo,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                    CCS = u.CCS,
                    Metadata = includeMetadata && u.Metadata != null
                        ? u.Metadata.Select(m => new TicketMetadata
                        {
                            Id = m.Id,
                            TicketId = m.TicketId,
                            Key = m.Key,
                            Value = m.Value
                        }).ToList()
                        : new List<TicketMetadata>()
                })
                .ToListAsync();

            return (tickets, totalCount);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while searching tickets.", ex);
        }
    }
    #endregion

    #region Update Ticket Attachment
    /// <inheritdoc/>
    public async Task UpdateTicketAttachmentAsync(
     int ticketId,
     List<IFormFile> formFiles,
     string fileName)
    {
        if (formFiles == null || formFiles.Count == 0 || formFiles.Count > 2)
        {
            throw new ArgumentException("Must upload 1 or 2 new files.");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Existing file name is required.");
        }

        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            throw new ArgumentException("Invalid file name.");
        }

        var ticketDirectory = Path.Combine("UploadedFiles", $"Ticket No.{ticketId}");
        var fullPathToDelete = Path.Combine(ticketDirectory, fileName);

        await using var context = _contextFactory.CreateDbContext();
        var ticket = await GetTicketByIdAsync(ticketId);

        bool attachmentExistsInDb = ticket.Attachments.Contains(fullPathToDelete);

        if (!attachmentExistsInDb)
        {
            throw new FileNotFoundException("The specified attachment does not exist in the ticket.");
        }

        try
        {
            await DeleteFileWithRetryAsync(fullPathToDelete, maxRetries: 5, delayMs: 500);

            ticket.Attachments.Remove(fileName);

            var newFileNames = formFiles.Select(f => f.FileName).ToList();

            if (newFileNames.Distinct(StringComparer.OrdinalIgnoreCase).Count() != newFileNames.Count)
            {
                throw new InvalidOperationException("Cannot upload multiple files with the same name.");
            }

            var remainingAttachments = ticket.Attachments.Except(new[] { fileName }, StringComparer.OrdinalIgnoreCase);
            var conflict = newFileNames.FirstOrDefault(name => remainingAttachments.Contains(name, StringComparer.OrdinalIgnoreCase));
            if (conflict != null)
            {
                throw new InvalidOperationException($"A file named '{conflict}' already exists in this ticket.");
            }

            if (formFiles.Count == 2 && FilesAreEqual(formFiles[0], formFiles[1]))
            {
                throw new InvalidOperationException("The two uploaded files have identical content.");
            }

            var uploadedFileNames = await UploadFilesAsync(formFiles, ticketId);

            ticket.Attachments.AddRange(uploadedFileNames);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to update ticket attachment.", ex);
        }
    }

    public async Task UpdateTicketAttachmentAsync(int ticketId, List<IFormFile> formFiles)
    {
        if (formFiles == null || formFiles.Count == 0)
        {
            throw new ArgumentException("No files provided.");
        }

        if (formFiles.Count > 2)
        {
            throw new InvalidOperationException("Maximum of 2 files can be uploaded at once.");
        }

        var context = _contextFactory.CreateDbContext();
        var ticket = await GetTicketByIdAsync(ticketId);
        try
        {
            var newFileNames = formFiles.Select(f => f.FileName).ToList();
            if (newFileNames.Distinct().Count() != newFileNames.Count)
            {
                throw new InvalidOperationException("Duplicate file names detected in the upload");
            }

            var existingNames = ticket.Attachments.Select(a => Path.GetFileName(a)).ToHashSet();
            var conflicting = newFileNames.FirstOrDefault(name => existingNames.Contains(name));
            if (conflicting != null)
            {
                throw new InvalidOperationException($"File '{conflicting}' already exists in the ticket");
            }

            if (formFiles.Count == 2)
            {
                if (FilesAreEqual(formFiles[0], formFiles[1]))
                {
                    throw new InvalidOperationException("The two files have identical content");
                }
            }

            var uploadedFileNames = await UploadFilesAsync(formFiles, ticketId);
            ticket.Attachments.AddRange(uploadedFileNames);

            context.Tickets.Update(ticket);
            await context.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Failed to update ticket attachment", exception);
        }
    }
    #endregion

    #region Update Ticket Title
    public async Task<Ticket> UpdateTicketTitleAsync(int ticketId, UpdateTitleDTO updateTitleDTO)
    {
        try
        {
            var context = _contextFactory.CreateDbContext();
            var ticketTitle = await GetTicketByIdAsync(ticketId);
            if (!string.IsNullOrWhiteSpace(updateTitleDTO.Title))
                ticketTitle.Title = updateTitleDTO.Title;

            context.Tickets.Update(ticketTitle);
            await context.SaveChangesAsync();
            return ticketTitle;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Failed to update ticket title", exception);
        }
    }
    #endregion

    #region Update Ticket Status
    public async Task<Ticket> UpdateTicketStatusAsync(int ticketId, TicketStatus status)
    {
        var ticket = await UpdateTicketFieldAsync(ticketId, "TicketStatus", status.ToString());
        return ticket;
    }

    public async Task<Ticket> UpdateTicketStatusAsync(int ticketId, string status)
    {
        if (Enum.TryParse<TicketStatus>(status, true, out var parsedStatus))
        {
            var ticket = await UpdateTicketStatusAsync(ticketId, parsedStatus);
            return ticket;
        }
        else
        {
            throw new ArgumentException($"Invalid status value: {status}", nameof(status));
        }
    }
    #endregion

    #region Update Ticket Priority
    public async Task<Ticket> UpdateTicketPriorityAsync(int ticketId, Priority priority)
    {
        var ticket = await UpdateTicketFieldAsync(ticketId, "Priority", priority.ToString());
        return ticket;
    }

    public async Task<Ticket> UpdateTicketPriorityAsync(int ticketId, string priority)
    {
        if (Enum.TryParse<Priority>(priority, true, out var parsedPriority))
        {
            var ticket = await UpdateTicketPriorityAsync(ticketId, parsedPriority);
            return ticket;
        }
        else
        {
            throw new ArgumentException($"Invalid priority value: {priority}", nameof(priority));
        }
    }
    #endregion

    #region Update Ticket Tag
    public async Task<Ticket> UpdateTicketTagAsync(int ticketId, Tags tags)
    {
        var ticket = await UpdateTicketFieldAsync(ticketId, "Tags", tags.ToString());
        return ticket;
    }

    public async Task<Ticket> UpdateTicketTagAsync(int ticketId, string tags)
    {
        if (Enum.TryParse<Tags>(tags, true, out var parsedTag))
        {
            var ticket = await UpdateTicketTagAsync(ticketId, parsedTag);
            return ticket;
        }
        else
        {
            throw new ArgumentException($"Invalid tag value {tags}", nameof(tags));
        }
    }
    #endregion

    #region Update ticket field
    protected async Task<Ticket> UpdateTicketFieldAsync(int ticketId, string fieldName, string value)
    {
        if (ticketId <= 0)
            throw new ArgumentException("Ticket ID must be greater than zero.", nameof(ticketId));

        try
        {
            using var context = _contextFactory.CreateDbContext();
            var ticket = await context.Tickets.FirstOrDefaultAsync(u => u.TicketId == ticketId);

            if (ticket == null)
                throw new InvalidOperationException($"Ticket with ID {ticketId} not found.");


            switch (fieldName)
            {
                case "TicketStatus":
                    ticket.TicketStatus = value;
                    break;
                case "Priority":
                    ticket.Priority = value;
                    break;
                case "Tags":
                    ticket.Tag = value;
                    break;
                default:
                    throw new ArgumentException($"Invalid field name: {fieldName}", nameof(fieldName));
            }
            context.Tickets.Update(ticket);
            await context.SaveChangesAsync();
            return ticket;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An unexpected error occurred while updating ticket {fieldName}.", ex);
        }
    }
    #region Update ticket Description
    public async Task<Ticket> UpdateTicketDescriptionAsync(int ticketId, UpdateDescriptionDTO updateDescriptionDTO)
    {
        try
        {
            var context = _contextFactory.CreateDbContext();
            var ticketDescription = await GetTicketByIdAsync(ticketId);
            if (!string.IsNullOrWhiteSpace(updateDescriptionDTO.Description))
                ticketDescription.Description = updateDescriptionDTO.Description;

            context.Tickets.Update(ticketDescription);
            await context.SaveChangesAsync();
            return ticketDescription;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Failed to update ticket description", exception);
        }
    }
    #endregion

    #region Update ticket assignee
    public async Task<Ticket> UpdateTicketAssigneeAsync(int ticketId, UpdateAssigneeDTO updateAssigneeDTO)
    {
        try
        {
            var context = _contextFactory.CreateDbContext();
            var ticketAssignee = await GetTicketByIdAsync(ticketId);
            if (!string.IsNullOrWhiteSpace(updateAssigneeDTO.AssigneeEmail))
                ticketAssignee.AssigneeEmail = updateAssigneeDTO.AssigneeEmail;

            context.Tickets.Update(ticketAssignee);
            await context.SaveChangesAsync();
            return ticketAssignee;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Failed to update ticket assignee", exception);
        }
    }

    #region Get Tickets By Status
    public async Task<IEnumerable<Ticket>> GetTicketByStatusAsync(string status, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be null or empty.", nameof(status));

        if (pageNumber <= 0)
            throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));

        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

        try
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Tickets.AsNoTracking().Where(u => u.TicketStatus == status.ToString());

            if (includeMetadata)
            {
                query = query.Include(u => u.Metadata);
            }

            var skip = (pageNumber - 1) * pageSize;
            var tickets = await query
                .OrderBy(u => u.TicketId)
                .Skip(skip)
                .Take(pageSize)
                .Select(u => new Ticket
                {
                    TicketId = u.TicketId,
                    AssigneeEmail = u.AssigneeEmail,
                    Description = u.Description,
                    PromiseDate = u.PromiseDate,
                    TicketStatus = u.TicketStatus,
                    Title = u.Title,
                    Attachments = u.Attachments,
                    CreatorEmail = u.CreatorEmail,
                    Priority = u.Priority,
                    Tag = u.Tag,
                    AdditionalInfo = u.AdditionalInfo,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                    CCS = u.CCS,
                    Metadata = includeMetadata && u.Metadata != null
                        ? u.Metadata.Select(m => new TicketMetadata
                        {
                            Id = m.Id,
                            TicketId = m.TicketId,
                            Key = m.Key,
                            Value = m.Value
                        }).ToList()
                        : new List<TicketMetadata>()
                })
                .ToListAsync();

            return tickets;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving tickets by status.", ex);
        }
    }

    public async Task<IEnumerable<Ticket>> GetTicketByStatusAsync(TicketStatus status, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20)
    {
        return await GetTicketByStatusAsync(status.ToString(), includeMetadata, pageNumber, pageSize);
    }
    #endregion

    #region Get Number of Tickets
    public async Task<int> GetNumberOfTicketsAsync()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await GetTotalTicketCountQuery(context);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while getting the number of tickets.", ex);
        }
    }
    #endregion

    #region Get Number of Tickets-Status
    public async Task<int> GetNumberOfTicketsByStatusAsync(TicketStatus status)
    {
        return await GetNumberOfTicketsByStatusAsync(status.ToString());
    }

    public async Task<int> GetNumberOfTicketsByStatusAsync(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be null or empty.", nameof(status));

        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets.AsNoTracking().CountAsync(c => c.TicketStatus == status);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while getting the number of tickets by status.", ex);
        }
    }
    #endregion

    #region Filter ticket
    public async Task<List<UpdateTicketDTO>> FilterTicketBy(Expression<Func<Ticket, bool>> filter)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();

            var tickets = await context.Tickets.Where(filter).ToListAsync();


            var ticketsFilter = tickets.Select(ticket => new UpdateTicketDTO()
            {
                Title = ticket.Title,
                Description = ticket.Description,
                AssigneeEmail = ticket.AssigneeEmail,
                TicketStatus = ticket.TicketStatus,
                Tag = ticket.Tag,
                Priority = ticket.Priority,
                CreatorEmail = ticket.CreatorEmail,
                AdditionalInfo = ticket.AdditionalInfo,
                PromiseDate = ticket.PromiseDate,
                CCS = ticket.CCS,
            });
            return ticketsFilter.ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get ticket(s)", ex);
        }
    }
    #endregion

    #region Get Ticket by Priority
    public async Task<IEnumerable<Ticket>> GetTicketByPriorityAsync(string priority, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(priority))
            throw new ArgumentException("Priority cannot be null or empty.", nameof(priority));

        if (pageNumber <= 0)
            throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));

        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

        try
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Tickets.AsNoTracking().Where(u => u.Priority == priority.ToString());

            if (includeMetadata)
            {
                query = query.Include(u => u.Metadata);
            }

            var skip = (pageNumber - 1) * pageSize;
            var tickets = await query
                .OrderBy(u => u.TicketId)
                .Skip(skip)
                .Take(pageSize)
                .Select(u => new Ticket
                {
                    TicketId = u.TicketId,
                    AssigneeEmail = u.AssigneeEmail,
                    Description = u.Description,
                    PromiseDate = u.PromiseDate,
                    TicketStatus = u.TicketStatus,
                    Title = u.Title,
                    Attachments = u.Attachments,
                    CreatorEmail = u.CreatorEmail,
                    Priority = u.Priority,
                    Tag = u.Tag,
                    AdditionalInfo = u.AdditionalInfo,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                    CCS = u.CCS,
                    Metadata = includeMetadata && u.Metadata != null
                        ? u.Metadata.Select(m => new TicketMetadata
                        {
                            Id = m.Id,
                            TicketId = m.TicketId,
                            Key = m.Key,
                            Value = m.Value
                        }).ToList()
                        : new List<TicketMetadata>()
                })
                .ToListAsync();

            return tickets;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving tickets by priority.", ex);
        }
    }

    public async Task<IEnumerable<Ticket>> GetTicketByPriorityAsync(Priority priority, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20)
    {
        return await GetTicketByPriorityAsync(priority.ToString(), includeMetadata, pageNumber, pageSize);
    }
    #endregion

    #region Number of tickets by priority
    public async Task<int> GetNumberOfTicketsByPriorityAsync(Priority priority)
    {
        return await GetNumberOfTicketsByPriorityAsync(priority.ToString());
    }

    public async Task<int> GetNumberOfTicketsByPriorityAsync(string priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
            throw new ArgumentException("Priority cannot be null or empty.", nameof(priority));

        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets.AsNoTracking().CountAsync(c => c.Priority == priority);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while getting the number of tickets by priority", ex);
        }
    }
    #endregion

    #region Get ticket by tags
    public async Task<IEnumerable<Ticket>> GetTicketByTagAsync(string tag, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be null or empty.", nameof(tag));

        if (pageNumber <= 0)
            throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));

        if (pageSize <= 0)
            throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

        try
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Tickets.AsNoTracking().Where(u => u.Tag == tag.ToString());

            if (includeMetadata)
            {
                query = query.Include(u => u.Metadata);
            }

            var skip = (pageNumber - 1) * pageSize;
            var tickets = await query
                .OrderBy(u => u.TicketId)
                .Skip(skip)
                .Take(pageSize)
                .Select(u => new Ticket
                {
                    TicketId = u.TicketId,
                    AssigneeEmail = u.AssigneeEmail,
                    Description = u.Description,
                    PromiseDate = u.PromiseDate,
                    TicketStatus = u.TicketStatus,
                    Title = u.Title,
                    Attachments = u.Attachments,
                    CreatorEmail = u.CreatorEmail,
                    Priority = u.Priority,
                    Tag = u.Tag,
                    AdditionalInfo = u.AdditionalInfo,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                    CCS = u.CCS,
                    Metadata = includeMetadata && u.Metadata != null
                        ? u.Metadata.Select(m => new TicketMetadata
                        {
                            Id = m.Id,
                            TicketId = m.TicketId,
                            Key = m.Key,
                            Value = m.Value
                        }).ToList()
                        : new List<TicketMetadata>()
                })
                .ToListAsync();

            return tickets;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving tickets by tag", ex);
        }
    }

    public async Task<IEnumerable<Ticket>> GetTicketByTagAsync(Tags tag, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20)
    {
        return await GetTicketByTagAsync(tag.ToString(), includeMetadata, pageNumber, pageSize);
    }
    #endregion

    #region Number of tickets by tags
    public async Task<int> GetNumberOfTicketsByTagAsync(Tags tag)
    {
        return await GetNumberOfTicketsByTagAsync(tag.ToString());
    }

    public async Task<int> GetNumberOfTicketsByTagAsync(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be null or empty.", nameof(tag));

        try
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Tickets.AsNoTracking().CountAsync(c => c.Tag == tag);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while getting the number of tickets by tag", ex);
        }
    }
    #endregion

    // #region Executing queries
    // /// <inheritdoc/>
    // public virtual async Task<List<T>> ExecuteRawQueryAsync<T>(string sql, params object[] parameters) where T : class
    // {
    //     if (string.IsNullOrWhiteSpace(sql))
    //         throw new ArgumentException("SQL query cannot be null or empty.", nameof(sql));

    //     try
    //     {
    //         using var context = _contextFactory.CreateDbContext();
    //         return await context.Set<T>().FromSqlRaw(sql, parameters).ToListAsync();
    //     }
    //     catch (Exception ex)
    //     {
    //         throw new InvalidOperationException("An error occurred while executing raw query.", ex);
    //     }
    // }
    // /// <inheritdoc/>
    // public virtual async Task<List<T>> ExecuteRawScalarQueryAsync<T>(string sql, params object[] parameters)
    // {
    //     if (string.IsNullOrWhiteSpace(sql))
    //         throw new ArgumentException("SQL query cannot be null or empty.", nameof(sql));

    //     try
    //     {
    //         using var context = _contextFactory.CreateDbContext();
    //         using var command = context.Database.GetDbConnection().CreateCommand();
    //         command.CommandText = sql;

    //         // Add parameters
    //         foreach (var param in parameters)
    //         {
    //             var dbParam = command.CreateParameter();
    //             dbParam.Value = param ?? DBNull.Value;
    //             command.Parameters.Add(dbParam);
    //         }

    //         await context.Database.OpenConnectionAsync();
    //         using var reader = await command.ExecuteReaderAsync();

    //         var results = new List<T>();
    //         while (await reader.ReadAsync())
    //         {
    //             var value = reader.GetValue(0);
    //             if (value != DBNull.Value)
    //             {
    //                 results.Add((T)Convert.ChangeType(value, typeof(T)));
    //             }
    //         }

    //         return results;
    //     }
    //     catch (Exception ex)
    //     {
    //         throw new InvalidOperationException("An error occurred while executing raw scalar query.", ex);
    //     }
    // }
    // /// <inheritdoc/>
    // public virtual async Task<int> ExecuteRawNonQueryAsync(string sql, params object[] parameters)
    // {
    //     if (string.IsNullOrWhiteSpace(sql))
    //         throw new ArgumentException("SQL command cannot be null or empty.", nameof(sql));

    //     try
    //     {
    //         using var context = _contextFactory.CreateDbContext();
    //         return await context.Database.ExecuteSqlRawAsync(sql, parameters);
    //     }
    //     catch (Exception ex)
    //     {
    //         throw new InvalidOperationException("An error occurred while executing raw non-query command.", ex);
    //     }
    // }
    // #endregion
    #endregion

    #region Dispose Pattern
    /// <summary>
    /// Disposes the resources used by the BaseDbImplementation.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the BaseDbImplementation.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_IsDisposed && disposing)
        {
            // Dispose managed resources if any
            _IsDisposed = true;
        }
    }
    #endregion
    #endregion
}

