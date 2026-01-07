using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NSCatch.Interfaces;
using NSCore.DatabaseContext;
using NSCore.DatabaseProviders;
using TicketManager.Data;
using TicketManager.DbImplementations;
using TicketManager.DTOs;
using TicketManager.Enumerations;
using TicketManager.Interfaces;
using TicketManager.LazyLoading;
using TicketManager.Models;
using TicketManager.Services;

namespace TicketManager;

/// <summary>
/// Manages ticket-related operations such as adding, deleting, and updating tickets
/// </summary>
public class ManageTickets : ManageTickets<TicketStatus, Priority, Tags>
{
    /// <summary>
    /// Initializes a new instance of the ManageTickets class.
    /// </summary>
    /// <param name="config">The database configuration.</param>
    /// <param name="contextFactory">The database context factory.</param>
    /// <param name="cacheManager">The cache manager instance.</param>
    /// <param name="keyBuilder">The cache key builder instance.</param>
    /// <param name="applyMigrationsAutomatically">Whether to apply migrations automatically.</param>
    public ManageTickets(IDatabaseConfig config, IDbContextFactory<AppDbContext> contextFactory,
    ICacheManager cacheManager, ICacheKeyBuilder keyBuilder, bool applyMigrationsAutomatically = true)
    : base(config, contextFactory, cacheManager, keyBuilder, applyMigrationsAutomatically) { }
}

/// <summary>
/// 
/// </summary>
public class ManageTickets<TicketStatus, Priority, Tags> : BackgroundService, ITicketManager<TicketStatus, Priority, Tags>
where TicketStatus : struct, Enum
where Priority : struct, Enum
where Tags : struct, Enum
{
    /// <summary>
    /// 
    /// </summary>
    private ITicketManager<TicketStatus, Priority, Tags> _connection;
    private bool _isContextCreated;
    private bool _applyMigrationsAutomatically;
    private INsContextInit _initializer;
    private readonly ICacheManager _cacheManager;
    private readonly ICacheKeyBuilder _keyBuilder;
    private LazyLoadManager _ticketLoadManager;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="config"></param>
    /// <param name="contextFactory"></param>
    /// <param name="cacheManager"></param>
    /// <param name="cacheKeyBuilder"></param>
    /// <param name="applyMigrationsAutomatically"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public ManageTickets(IDatabaseConfig config, IDbContextFactory<AppDbContext> contextFactory, ICacheManager cacheManager, ICacheKeyBuilder cacheKeyBuilder, bool applyMigrationsAutomatically = true)
    {
        _applyMigrationsAutomatically = applyMigrationsAutomatically;
        this._cacheManager = cacheManager;
        this._keyBuilder = cacheKeyBuilder;

        _connection = config switch
        {
            PSQLDb => new PostgreSQL<TicketStatus, Priority, Tags>(contextFactory),
            MySQLDb => new MySQL<TicketStatus, Priority, Tags>(contextFactory),
            SQLDb => new SQLServer<TicketStatus, Priority, Tags>(contextFactory),
            _ => throw new ArgumentException("Unsupported database type.")
        };

        if (_connection == null)
            throw new InvalidOperationException("Failed to initialize a valid INsContextInitializer.");
        _initializer = _connection as INsContextInit ?? throw new InvalidOperationException("Failed to initialize a valid INsContextInitializer.");
        InitializeLazyLoadingSystem();
    }

    ///<inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_connection == null)
        {
            throw new InvalidOperationException("Database connection is not initialized.");
        }

        if (_applyMigrationsAutomatically)
        {
            await _initializer.ApplyMigrationsAsync(stoppingToken);
        }

        if (_initializer.IsContextCreated())
        {
            _isContextCreated = true;
        }
    }
    private void InitializeLazyLoadingSystem()
    {
        var dataService = new DataService(pageSize: 10, fetchDataFunc: (from, pageSize) => FetchTicketsAsync(from, pageSize, includeMetadata: false));
        _ticketLoadManager = new LazyLoadManager(dataService);
    }

    public Task<List<Ticket>> FetchTicketsAsync(int from = 0, int pageSize = 10, bool includeMetadata = false)
    {
        EnsureContext();
        // return _cacheManager.GetOrSetAsync<List<Ticket>>(key: _keyBuilder.BuildKey("GeneralKey"), () => _connection.FetchTicketsAsync(from, pageSize, includeMetadata));
        return _connection.FetchTicketsAsync(from, pageSize, includeMetadata);
    }

    #region Create ticket
    ///<inheritdoc/>
    public async Task<Ticket> CreateTicketAsync(CreateTicketDTO createTicketDTO, List<IFormFile> files)
    {
        EnsureContext();
        Ticket createdTicket;
        createdTicket = await _connection.CreateTicketAsync(createTicketDTO, files);

        // var generalCacheKey = _keyBuilder.BuildKey("GeneralKey");
        // var cachedTickets = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        // if (cachedTickets != null)
        // {
        //     var updatedTickets = cachedTickets.Concat(new[] { createdTicket }).ToList();
        //     await _cacheManager.SetAsync(generalCacheKey, updatedTickets);
        // }
        return createdTicket;
    }
    #endregion

    #region Get ticket by ID
    ///<inheritdoc/>
    public async Task<Ticket> GetTicketByIdAsync(int ticketId, bool includeMetadata = false)
    {
        EnsureContext();
        // return await _cacheManager.GetOrSetAsync<Ticket>(_keyBuilder.BuildKey<TicketStatus>(ticketId), () => _connection
        // .GetTicketByIdAsync(ticketId, includeMetadata));
        return await _connection.GetTicketByIdAsync(ticketId, includeMetadata);
    }
    #endregion

    #region Get all tickets
    ///<inheritdoc/>
    public async Task<List<Ticket>> GetAllTicketsAsync(int pageNumber = 1, int pageSize = 20, bool includeMetadata = false)
    {
        EnsureContext();
        // return await _cacheManager.GetOrSetAsync<List<Ticket>>(key: _keyBuilder.BuildKey("GeneralKey"), () => _connection
        // .GetAllTicketsAsync(pageNumber: pageNumber, pageSize: pageSize, includeMetadata: includeMetadata));
        return await _connection.GetAllTicketsAsync(pageNumber: pageNumber, pageSize: pageSize, includeMetadata: includeMetadata);
    }
    #endregion

    #region Upload file
    ///<inheritdoc/>
    public async Task<List<string>> UploadFilesAsync(List<IFormFile> formFiles, int ticketId)
    {
        EnsureContext();
        return await _connection.UploadFilesAsync(formFiles, ticketId);
    }
    #endregion

    #region Delete ticket attachment
    ///<inheritdoc/>
    public async Task DeleteAttachmentAsync(int ticketId, string fileName)
    {
        EnsureContext();
        // var ticketCacheKey = _keyBuilder.BuildKey<TicketStatus>(ticketId);
        // await _cacheManager.RemoveAsync(ticketCacheKey);

        // var generalCacheKey = _keyBuilder.BuildKey("GeneralKey");
        // var cachedTicket = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        // if (cachedTicket != null)
        // {
        //     var attachmentToDelete = cachedTicket.Where(x => x.TicketId != ticketId).ToList();
        //     await _cacheManager.SetAsync(generalCacheKey, attachmentToDelete);
        // }
        // else
        // {
        //     await _cacheManager.RemoveAsync(generalCacheKey);
        // }
        await _connection.DeleteAttachmentAsync(ticketId, fileName);
    }
    #endregion

    #region Delete ticket
    ///<inheritdoc/>
    public async Task DeleteTicketAsync(int ticketId)
    {
        EnsureContext();
        // var ticketCacheKey = _keyBuilder.BuildKey<TicketStatus>(ticketId);
        // await _cacheManager.RemoveAsync(ticketCacheKey);

        // var generalCacheKey = _keyBuilder.BuildKey("GeneralKey");
        // var cachedTicket = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        // if (cachedTicket != null)
        // {
        //     var ticketToDelete = cachedTicket.Where(x => x.TicketId != ticketId).ToList();
        //     await _cacheManager.SetAsync(generalCacheKey, ticketToDelete);
        // }
        // else
        // {
        //     await _cacheManager.RemoveAsync(generalCacheKey);
        // }
        await _connection.DeleteTicketAsync(ticketId);
    }
    #endregion

    #region Search Tickets
    public async Task<(IEnumerable<Ticket> Tickets, int TotalCount)> SearchTicketsAsync(string? searchTerm, string tag, string priority, string? status, string sortBy, bool ascending, int page, int pageSize, SearchMode searchMode, bool includeMetadata = false)
    {
        EnsureContext();

        var searchCacheKey = _keyBuilder.BuildKey(searchTerm, priority, status, sortBy, ascending, page, pageSize, searchMode);
        var result = await _cacheManager.GetOrSetAsync(searchCacheKey, async () =>
        {
            var tuppleResult = await _connection.SearchTicketsAsync(searchTerm, priority, status, sortBy, tag, ascending, page, pageSize, searchMode, includeMetadata);
            return new SearchTicketResult(tuppleResult.Tickets, tuppleResult.TotalCount);
        });
        return (result.Tickets, result.TotalCount);
    }
    #endregion

    #region Update ticket
    ///<inheritdoc/>
    public async Task<Ticket> UpdateTicketAsync(int ticketId, UpdateTicketDTO updateTicketDTO)
    {
        EnsureContext();

        Ticket updatedTicket;
        updatedTicket = await _connection.UpdateTicketAsync(ticketId, updateTicketDTO);

        // var ticketCacheKey = _keyBuilder.BuildKey<TicketStatus>(ticketId);
        // await _cacheManager.RemoveAsync(ticketCacheKey);

        // var generalCacheKey = _keyBuilder.BuildKey("GeneralKey");
        // var cachedTicket = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        // if (cachedTicket != null)
        // {
        //     var updatedTickets = cachedTicket.Where(u => u.TicketId != ticketId).Concat(new[] { updatedTicket }).ToList();
        //     await _cacheManager.SetAsync(generalCacheKey, updatedTickets);
        // }

        return updatedTicket;
    }
    #endregion

    #region Update ticket attachment
    ///<inheritdoc/>
    public async Task UpdateTicketAttachmentAsync(int ticketId, List<IFormFile> formFiles, string fileName)
    {
        EnsureContext();
        await _connection.UpdateTicketAttachmentAsync(ticketId, formFiles, fileName);
    }

    ///<inheritdoc/>
    public async Task UpdateTicketAttachmentAsync(int ticketId, List<IFormFile> formFiles)
    {
        EnsureContext();
        await _connection.UpdateTicketAttachmentAsync(ticketId, formFiles);
    }
    #endregion

    #region Update ticket title
    ///<inheritdoc/>
    public async Task<Ticket> UpdateTicketTitleAsync(int ticketId, UpdateTitleDTO updateTitleDTO)
    {
        EnsureContext();

        Ticket updatedTicket;
        updatedTicket = await _connection.UpdateTicketTitleAsync(ticketId, updateTitleDTO);

        // var ticketCacheKey = _keyBuilder.BuildKey<TicketStatus>(ticketId);
        // await _cacheManager.RemoveAsync(ticketCacheKey);

        // var generalCacheKey = _keyBuilder.BuildKey("GeneralKey");
        // var cachedTickets = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        // if (cachedTickets != null)
        // {
        //     var updatedTickets = cachedTickets.Where(u => u.TicketId != ticketId).Concat(new[] { updatedTicket }).ToList();
        //     await _cacheManager.SetAsync(generalCacheKey, updatedTickets);
        // }
        return updatedTicket;
    }
    #endregion


    #region Update ticket status
    ///<inheritdoc/>
    public async Task<Ticket> UpdateTicketStatusAsync(int ticketId, TicketStatus ticketStatus)
    {
        EnsureContext();

        Ticket updatedTicket;
        updatedTicket = await _connection.UpdateTicketStatusAsync(ticketId, ticketStatus);

        // var ticketCacheKey = _keyBuilder.BuildKey<TicketStatus>(ticketId);
        // await _cacheManager.RemoveAsync(ticketCacheKey);

        // var generalCacheKey = _keyBuilder.BuildKey("GeneralKey");
        // var cachedTickets = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        // if (cachedTickets != null)
        // {
        //     var updatedTickets = cachedTickets.Where(u => u.TicketId != ticketId).Concat(new[] { updatedTicket }).ToList();
        //     await _cacheManager.SetAsync(generalCacheKey, updatedTickets);
        // }
        return updatedTicket;
    }

    ///<inheritdoc/>
    public async Task<Ticket> UpdateTicketStatusAsync(int ticketId, string status)
    {
        EnsureContext();

        Ticket updatedTicket;
        updatedTicket = await _connection.UpdateTicketStatusAsync(ticketId, status);

        // var ticketCacheKey = _keyBuilder.BuildKey<TicketStatus>(ticketId);
        // await _cacheManager.RemoveAsync(ticketCacheKey);

        // var generalCacheKey = _keyBuilder.BuildKey("GeneralKey");
        // var cachedTickets = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        // if (cachedTickets != null)
        // {
        //     var updatedTickets = cachedTickets.Where(u => u.TicketId != ticketId).Concat(new[] { updatedTicket }).ToList();
        //     await _cacheManager.SetAsync(generalCacheKey, updatedTickets);
        // }
        return updatedTicket;
    }
    #endregion

    #region Update ticket description
    /// <inheritdoc/>
    public async Task<Ticket> UpdateTicketDescriptionAsync(int ticketId, UpdateDescriptionDTO updateDescriptionDTO)
    {
        EnsureContext();

        Ticket updatedTicket;
        updatedTicket = await _connection.UpdateTicketDescriptionAsync(ticketId, updateDescriptionDTO);

        // var ticketCacheKey = _keyBuilder.BuildKey<TicketStatus>(ticketId);
        // await _cacheManager.RemoveAsync(ticketCacheKey);

        // var generalCacheKey = _keyBuilder.BuildKey("GeneralKey");
        // var cachedTickets = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        // if (cachedTickets != null)
        // {
        //     var updatedTickets = cachedTickets.Where(u => u.TicketId != ticketId).Concat(new[] { updatedTicket }).ToList();
        //     await _cacheManager.SetAsync(generalCacheKey, updatedTickets);
        // }
        return updatedTicket;
    }
    #endregion

    #region Update ticket assignee
    /// <inheritdoc/>
    public async Task<Ticket> UpdateTicketAssigneeAsync(int ticketId, UpdateAssigneeDTO updateAssigneeDTO)
    {
        EnsureContext();

        Ticket updatedTicket;
        updatedTicket = await _connection.UpdateTicketAssigneeAsync(ticketId, updateAssigneeDTO);

        // var ticketCacheKey = _keyBuilder.BuildKey<TicketStatus>(ticketId);
        // await _cacheManager.RemoveAsync(ticketCacheKey);

        // var generalCacheKey = _keyBuilder.BuildKey("GeneralKey");
        // var cachedTickets = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        // if (cachedTickets != null)
        // {
        //     var updatedTickets = cachedTickets.Where(u => u.TicketId != ticketId).Concat(new[] { updatedTicket }).ToList();
        //     await _cacheManager.SetAsync(generalCacheKey, updatedTickets);
        // }
        return updatedTicket;
    }
    #endregion

    #region Get ticket by status
    /// <inheritdoc/>
    public async Task<IEnumerable<Ticket>> GetTicketByStatusAsync(string status, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20)
    {
        EnsureContext();
        return await _cacheManager.GetOrSetAsync<IEnumerable<Ticket>>(key: _keyBuilder.BuildKey(status), () => _connection
        .GetTicketByStatusAsync(status, includeMetadata, pageNumber, pageSize));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Ticket>> GetTicketByStatusAsync(TicketStatus status, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20)
    {
        EnsureContext();
        return await _cacheManager.GetOrSetAsync<IEnumerable<Ticket>>(key: _keyBuilder.BuildKey(status.ToString()), () => _connection
        .GetTicketByStatusAsync(status, includeMetadata, pageNumber, pageSize));
    }
    #endregion

    #region Get number of tickets
    /// <inheritdoc/>
    public async Task<int> GetNumberOfTicketsAsync()
    {
        EnsureContext();
        return await _connection.GetNumberOfTicketsAsync();
    }
    #endregion

    #region Get number of tickets by status
    /// <inheritdoc/>
    public async Task<int> GetNumberOfTicketsByStatusAsync(TicketStatus status)
    {
        EnsureContext();
        return await _connection.GetNumberOfTicketsByStatusAsync(status);
    }

    /// <inheritdoc/>
    public async Task<int> GetNumberOfTicketsByStatusAsync(string status)
    {
        EnsureContext();
        return await _connection.GetNumberOfTicketsByStatusAsync(status);
    }
    #endregion

    #region Filter ticket
    public async Task<List<UpdateTicketDTO>> FilterTicketBy(Expression<Func<Ticket, bool>> filter)
    {
        EnsureContext();
        return await _connection.FilterTicketBy(filter);
    }
    #endregion

    #region Update ticket priority
    public async Task<Ticket> UpdateTicketPriorityAsync(int ticketId, Priority ticketPriority)
    {
        EnsureContext();
        Ticket updatedTicket;
        updatedTicket = await _connection.UpdateTicketPriorityAsync(ticketId, ticketPriority);

        var ticketCacheKey = _keyBuilder.BuildKey<Priority>(ticketId);
        await _cacheManager.RemoveAsync(ticketCacheKey);

        var generalCacheKey = _keyBuilder.BuildKey("GeneralKey");
        var cachedTickets = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        if (cachedTickets != null)
        {
            var updatedTickets = cachedTickets.Where(u => u.TicketId != ticketId).Concat(new[] { updatedTicket }).ToList();
            await _cacheManager.SetAsync(generalCacheKey, updatedTickets);
        }
        return updatedTicket;
    }

    public async Task<Ticket> UpdateTicketPriorityAsync(int ticketId, string priority)
    {
        EnsureContext();
        Ticket updatedTicket;
        updatedTicket = await _connection.UpdateTicketPriorityAsync(ticketId, priority);

        var ticketCacheKey = _keyBuilder.BuildKey<Priority>(ticketId);
        await _cacheManager.RemoveAsync(ticketCacheKey);

        var generalCacheKey = _keyBuilder.BuildKey("GeneralKey");
        var cachedTickets = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        if (cachedTickets != null)
        {
            var updatedTickets = cachedTickets.Where(u => u.TicketId != ticketId).Concat(new[] { updatedTicket }).ToList();
            await _cacheManager.SetAsync(generalCacheKey, updatedTickets);
        }
        return updatedTicket;
    }
    #endregion

    #region Get ticket by priority
    public async Task<IEnumerable<Ticket>> GetTicketByPriorityAsync(string priority, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20)
    {
        EnsureContext();
        return await _cacheManager.GetOrSetAsync<IEnumerable<Ticket>>(key: _keyBuilder.BuildKey(priority), () => _connection
        .GetTicketByPriorityAsync(priority, includeMetadata, pageNumber, pageSize)); ;
    }

    public async Task<IEnumerable<Ticket>> GetTicketByPriorityAsync(Priority priority, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20)
    {
        EnsureContext();
        return await _cacheManager.GetOrSetAsync<IEnumerable<Ticket>>(key: _keyBuilder.BuildKey(priority.ToString()), () => _connection
        .GetTicketByPriorityAsync(priority, includeMetadata, pageNumber, pageSize));
    }
    #endregion

    #region Get number of tickets by priority
    public async Task<int> GetNumberOfTicketsByPriorityAsync(Priority priority)
    {
        EnsureContext();
        return await _connection.GetNumberOfTicketsByPriorityAsync(priority);
    }

    public async Task<int> GetNumberOfTicketsByPriorityAsync(string priority)
    {
        EnsureContext();
        return await _connection.GetNumberOfTicketsByPriorityAsync(priority);
    }
    #endregion

    #region Update ticket tag

    public async Task<Ticket> UpdateTicketTagAsync(int ticketId, Tags ticketTag)
    {
        EnsureContext();
        Ticket updatedTicket;
        updatedTicket = await _connection.UpdateTicketTagAsync(ticketId, ticketTag);

        var ticketCacheKey = _keyBuilder.BuildKey<Tags>(ticketId);
        await _cacheManager.RemoveAsync(ticketCacheKey);

        var generalCacheKey = _keyBuilder.BuildKey("GeneralKey");
        var cachedTickets = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        if (cachedTickets != null)
        {
            var updatedTickets = cachedTickets.Where(u => u.TicketId != ticketId).Concat(new[] { updatedTicket }).ToList();
            await _cacheManager.SetAsync(generalCacheKey, updatedTickets);
        }
        return updatedTicket;
    }

    public async Task<Ticket> UpdateTicketTagAsync(int ticketId, string ticketTag)
    {
        EnsureContext();
        Ticket updatedTicket;
        updatedTicket = await _connection.UpdateTicketTagAsync(ticketId, ticketTag);

        var ticketCacheKey = _keyBuilder.BuildKey<Tags>(ticketId);
        await _cacheManager.RemoveAsync(ticketCacheKey);

        var generalCacheKey = _keyBuilder.BuildKey("GeneralKey");
        var cachedTickets = await _cacheManager.GetAsync<List<Ticket>>(generalCacheKey);

        if (cachedTickets != null)
        {
            var updatedTickets = cachedTickets.Where(u => u.TicketId != ticketId).Concat(new[] { updatedTicket }).ToList();
            await _cacheManager.SetAsync(generalCacheKey, updatedTickets);
        }
        return updatedTicket;
    }
    #endregion

    #region Get ticket by tag

    public async Task<IEnumerable<Ticket>> GetTicketByTagAsync(string tag, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20)
    {
        EnsureContext();
        return await _cacheManager.GetOrSetAsync<IEnumerable<Ticket>>(key: _keyBuilder.BuildKey(tag), () => _connection
        .GetTicketByTagAsync(tag, includeMetadata, pageNumber, pageSize));
    }

    public async Task<IEnumerable<Ticket>> GetTicketByTagAsync(Tags tag, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20)
    {
        EnsureContext();
        return await _cacheManager.GetOrSetAsync<IEnumerable<Ticket>>(key: _keyBuilder.BuildKey(tag.ToString()), () => _connection
        .GetTicketByTagAsync(tag, includeMetadata, pageNumber, pageSize));
    }
    #endregion

    #region Get number of tickets by tags

    public async Task<int> GetNumberOfTicketsByTagAsync(Tags tag)
    {
        EnsureContext();
        return await _connection.GetNumberOfTicketsByTagAsync(tag);
    }

    public async Task<int> GetNumberOfTicketsByTagAsync(string tag)
    {
        EnsureContext();
        return await _connection.GetNumberOfTicketsByTagAsync(tag);
    }
    #endregion

    // #region Execute raw queries
    // /// <inheritdoc/>
    // public async Task<List<T>> ExecuteRawQueryAsync<T>(string sql, params object[] parameters) where T : class
    // {
    //     EnsureContext();
    //     return await _connection.ExecuteRawQueryAsync<T>(sql, parameters);// For queries returning entity objects
    // }

    // /// <inheritdoc/>
    // public Task<List<T>> ExecuteRawScalarQueryAsync<T>(string sql, params object[] parameters)
    // {
    //     EnsureContext();
    //     return _connection.ExecuteRawScalarQueryAsync<T>(sql, parameters);// For queries returning simple scalar values
    // }

    // /// <inheritdoc/>
    // public Task<int> ExecuteRawNonQueryAsync(string sql, params object[] parameters)
    // {
    //     EnsureContext();
    //     return _connection.ExecuteRawNonQueryAsync(sql, parameters);//For commands that modify data
    // }
    // #endregion

    /// <summary>
    /// Ensures that the context has been properly initialized.
    /// Throws an InvalidOpertationException if the context is not created
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws an invalid operation exception</exception>
    private void EnsureContext()
    {
        if (!_isContextCreated)
        {
            throw new InvalidOperationException("Failed to initialize the database context.");
        }
    }
}
public record SearchTicketResult(IEnumerable<Ticket> Tickets, int TotalCount);




