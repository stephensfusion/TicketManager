using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketManager.DTOs;
using TicketManager.Models;

namespace TicketManager.Interfaces;

/// <summary>
/// Provides methods for managing tickets in the system
/// </summary>
public interface ITicketManager<TicketStatus, Priority, Tags>
{
    /// <summary>
    /// Adds a new ticket to the system using the CreateTicketDTO object
    /// </summary>
    /// <param name="createTicket">The CreateTicketDTO object containing ticket details</param>
    /// <param name="files">Uploads a file attachment to the ticket</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<Ticket> CreateTicketAsync(CreateTicketDTO createTicket, List<IFormFile> files);

    /// <summary>
    /// Uploads file attachment to the ticket
    /// </summary>
    /// <param name="formFiles">Uploads a file to the system</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<List<string>> UploadFilesAsync(List<IFormFile> formFiles, int ticketId);

    /// <summary>
    /// Retrieves a list of tickets starting from a specific index with optional metadata
    /// </summary>
    /// <param name="from">The zero-based index to start retrieving tickets from. Defaults to 0</param>
    /// <param name="pageSize">The number of tickets to retrieve. Defaults to 10</param>
    /// <param name="includeMetadata">Whether to include ticket metadata in the result. Defaults to false</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of tickets
    /// </returns>
    Task<List<Ticket>> FetchTicketsAsync(int from = 0, int pageSize = 10, bool includeMetadata = false);

    /// <summary>
    /// Searches for tickets in the system based on various criteria
    /// </summary>
    /// <param name="searchTerm">The term to search for in tickets fields (e.g., name, title). Optional</param>
    /// <param name="status">The status to filter tickets by. Optional</param>
    /// <param name="sortBy">The field to sort the results by. Defaults to "Name"</param>
    /// <param name="ascending">Whether to sort the results in ascending order. Defaults to true</param>
    /// <param name="pageNumber">The page number for pagination. Defaults to 1</param>
    /// <param name="pageSize">The number of results per page. Defaults to 10</param>
    /// <param name="searchMode">The search mode to use (e.g., Contains, StartsWith, etc.). Defaults to Contains</param>
    /// <param name="includeMetadata">Whether to include ticket metadata in the result. Defaults to false</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a tuple with the collection of tickets and the total count.
    /// </returns>
    Task<(IEnumerable<Ticket> Tickets, int TotalCount)> SearchTicketsAsync(
        string? searchTerm = null,
        string? status = null,
        string? tag = null,
        string? priority = null,
        string sortBy = "Name",
        bool ascending = true,
        int pageNumber = 1,
        int pageSize = 10,
        SearchMode searchMode = SearchMode.Contains, bool includeMetadata = false
        );

    /// <summary>
    /// Gets ticket by ID
    /// </summary>
    /// <param name="ticketId">Ticket unique identifier</param>
    /// <param name="includeMetadata">Whether to include ticket metadata in the result. Defaults to false.</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<Ticket> GetTicketByIdAsync(int ticketId, bool includeMetadata = false);

    /// <summary>
    /// Retrieves a paged list of tickets in the system.
    /// </summary>
    /// <param name="pageNumber">The page number for pagination. Defaults to 1.</param>
    /// <param name="pageSize">The number of results per page. Defaults to 10.</param>
    /// <param name="includeMetadata">Whether to include ticket metadata in the result. Defaults to false.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a paged result of tickets.
    /// </returns>
    Task<List<Ticket>> GetAllTicketsAsync(int pageNumber = 1, int pageSize = 20, bool includeMetadata = false);

    /// <summary>
    /// Retrieves a dynamically filtered list of tickets
    /// </summary>
    /// <param name="filter">Filter to be applied</param>
    /// <returns>A list of filtered tickets</returns>
    Task<List<UpdateTicketDTO>> FilterTicketBy(Expression<Func<Ticket, bool>> filter);

    /// <summary>
    /// Updates a ticket by ID
    /// </summary>
    /// <param name="ticketId">Ticket unique identifier</param>
    /// <param name="updateTicketDTO">The UpdateTicketDTO object containing ticket details</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<Ticket> UpdateTicketAsync(int ticketId, UpdateTicketDTO updateTicketDTO);

    /// <summary>
    /// Deletes ticket by ID
    /// </summary>
    /// <param name="ticketId">Ticket unique identifier</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteTicketAsync(int ticketId);

    /// <summary>
    /// Deletes a ticket attachment
    /// </summary>
    /// <param name="ticketId">Ticket unique identifier</param>
    /// <param name="nameOfFile">Deletes a file from the ticket using file name</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteAttachmentAsync(int ticketId, string nameOfFile);

    /// <summary>
    /// Updates a ticket attachment by overwriting existing ones
    /// </summary>
    /// <param name="ticketId">Ticket unique identifier</param>
    /// <param name="formFiles">Adds a file to a ticket</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateTicketAttachmentAsync(int ticketId, List<IFormFile> formFiles, string fileName);

    /// <summary>
    /// Updates a ticket attachment by adding file to existing ones
    /// </summary>
    /// <param name="ticketId">Ticket unique identifier</param>
    /// <param name="formFiles">Adds a file to a ticket</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateTicketAttachmentAsync(int ticketId, List<IFormFile> formFiles);

    /// <summary>
    /// Updates a ticket title 
    /// </summary>
    /// <param name="ticketId"></param>
    /// <param name="updateTitleDTO"></param>
    /// <returns></returns>
    Task<Ticket> UpdateTicketTitleAsync(int ticketId, UpdateTitleDTO updateTicketDTO);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ticketId"></param>
    /// <param name="updateDescriptionDTO"></param>
    /// <returns></returns>
    Task<Ticket> UpdateTicketDescriptionAsync(int ticketId, UpdateDescriptionDTO updateDescriptionDTO);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to update</param>
    /// <param name="updateAssigneeDTO"></param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<Ticket> UpdateTicketAssigneeAsync(int ticketId, UpdateAssigneeDTO updateAssigneeDTO);

    /// <summary>
    /// Updates the status of a ticket  
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to update</param>
    /// <param name="ticketStatus">The new status of the ticket</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<Ticket> UpdateTicketStatusAsync(int ticketId, TicketStatus ticketStatus);

    /// <summary>
    /// Updates the status of a ticket  
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to update</param>
    /// <param name="status">The new status of the ticket</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<Ticket> UpdateTicketStatusAsync(int ticketId, string status);

    ///<summary>
    /// Retrieves a paged list of ticket with the specified status
    /// </summary>
    /// <param name="status">The status to filter ticket by.</param>
    /// <param name="includeMetadata">Whether to include ticket metadata in the result. Defaults to false.</param>
    /// <param name="pageNumber">The page number for pagination. Defaults to 1.</param>
    /// <param name="pageSize">The number of results per page. Defaults to 20.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of ticket with the specified status
    /// </returns>
    Task<IEnumerable<Ticket>> GetTicketByStatusAsync(string status, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20);

    ///<summary>
    /// Retrieves a paged list of ticket with the specified status
    /// </summary>
    /// <param name="status">The status to filter ticket by</param>
    /// <param name="includeMetadata">Whether to include ticket metadata in the result. Defaults to false</param>
    /// <param name="pageNumber">The page number for pagination. Defaults to 1</param>
    /// <param name="pageSize">The number of results per page. Defaults to 20</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of ticket with the specified status
    /// </returns>
    Task<IEnumerable<Ticket>> GetTicketByStatusAsync(TicketStatus status, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20);

    /// <summary>
    /// Retrieves the total number of tickets in the system.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total number of tickets.</returns>
    Task<int> GetNumberOfTicketsAsync();

    ///<summary>
    /// Retrieves the total number of tickets in the system with the specified status
    /// </summary>
    /// <param name="status">The status to filter tickets by.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total number of tickets with the specified status.</returns>
    Task<int> GetNumberOfTicketsByStatusAsync(TicketStatus status);

    ///<summary>
    /// Retrieves the total number of tickets in the system with the specified status
    /// </summary>
    /// <param name="status">The role to filter tickets by.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total number of tickets with the specified status.</returns>
    Task<int> GetNumberOfTicketsByStatusAsync(string status);

    /// <summary>
    /// Updates the priority of a ticket  
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to update</param>
    /// <param name="ticketPriority">The new priority of the ticket</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<Ticket> UpdateTicketPriorityAsync(int ticketId, Priority ticketPriority);

    /// <summary>
    /// Updates the priority of a ticket  
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to update</param>
    /// <param name="priority">The new priority of the ticket</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<Ticket> UpdateTicketPriorityAsync(int ticketId, string priority);

    ///<summary>
    /// Retrieves a paged list of ticket with the specified priority
    /// </summary>
    /// <param name="priority">The priority to filter ticket by.</param>
    /// <param name="includeMetadata">Whether to include ticket metadata in the result. Defaults to false.</param>
    /// <param name="pageNumber">The page number for pagination. Defaults to 1.</param>
    /// <param name="pageSize">The number of results per page. Defaults to 20.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of ticket with the specified priority
    /// </returns>
    Task<IEnumerable<Ticket>> GetTicketByPriorityAsync(string priority, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20);

    ///<summary>
    /// Retrieves a paged list of ticket with the specified priority
    /// </summary>
    /// <param name="priority">The priority to filter ticket by.</param>
    /// <param name="includeMetadata">Whether to include ticket metadata in the result. Defaults to false.</param>
    /// <param name="pageNumber">The page number for pagination. Defaults to 1.</param>
    /// <param name="pageSize">The number of results per page. Defaults to 20.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of ticket with the specified priority
    /// </returns>
    Task<IEnumerable<Ticket>> GetTicketByPriorityAsync(Priority priority, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20);

    ///<summary>
    /// Retrieves the total number of tickets in the system with the specified priority
    /// </summary>
    /// <param name="priority">The priority to filter tickets by.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total number of tickets with the specified priority</returns>
    Task<int> GetNumberOfTicketsByPriorityAsync(Priority priority);

    ///<summary>
    /// Retrieves the total number of tickets in the system with the specified priority
    /// </summary>
    /// <param name="priority">The priority to filter tickets by.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total number of tickets with the specified priority</returns>
    Task<int> GetNumberOfTicketsByPriorityAsync(string priority);

    /// <summary>
    /// Updates the tag of a ticket  
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to update</param>
    /// <param name="ticketTag">The new tag of the ticket</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<Ticket> UpdateTicketTagAsync(int ticketId, Tags ticketTag);

    /// <summary>
    /// Updates the tag of a ticket  
    /// </summary>
    /// <param name="ticketId">The ID of the ticket to update</param>
    /// <param name="ticketTag">The new tag of the ticket</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task<Ticket> UpdateTicketTagAsync(int ticketId, string ticketTag);

    ///<summary>
    /// Retrieves a paged list of ticket with the specified tag
    /// </summary>
    /// <param name="tag">The tag to filter ticket by</param>
    /// <param name="includeMetadata">Whether to include ticket metadata in the result. Defaults to false</param>
    /// <param name="pageNumber">The page number for pagination. Defaults to 1.</param>
    /// <param name="pageSize">The number of results per page. Defaults to 20.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of ticket with the specified tag
    /// </returns>
    Task<IEnumerable<Ticket>> GetTicketByTagAsync(string tag, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20);

    ///<summary>
    /// Retrieves a paged list of ticket with the specified tag
    /// </summary>
    /// <param name="tag">The tag to filter ticket by</param>
    /// <param name="includeMetadata">Whether to include ticket metadata in the result. Defaults to false</param>
    /// <param name="pageNumber">The page number for pagination. Defaults to 1.</param>
    /// <param name="pageSize">The number of results per page. Defaults to 20.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of ticket with the specified tag
    /// </returns>
    Task<IEnumerable<Ticket>> GetTicketByTagAsync(Tags tag, bool includeMetadata = false, int pageNumber = 1, int pageSize = 20);

    ///<summary>
    /// Retrieves the total number of tickets in the system with the specified tag
    /// </summary>
    /// <param name="tag">The tag to filter tickets by</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total number of tickets with the specified tag</returns>
    Task<int> GetNumberOfTicketsByTagAsync(Tags tag);

    ///<summary>
    /// Retrieves the total number of tickets in the system with the specified tag
    /// </summary>
    /// <param name="tag">The tag to filter tickets by</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total number of tickets with the specified tag</returns>
    Task<int> GetNumberOfTicketsByTagAsync(string tag);

    // /// <summary>
    // /// Executes a raw SQL query with parameters to prevent SQL injection
    // /// </summary>
    // /// <typeparam name="T">The entity type to map results to</typeparam>
    // /// <param name="sql">SQL query with parameter placeholders</param>
    // /// <param name="parameters">SQL parameters to safely inject into query</param>
    // /// <returns>List of entities matching the query</returns>
    // Task<List<T>> ExecuteRawQueryAsync<T>(string sql, params object[] parameters) where T : class;

    // /// <summary>
    // /// Executes a raw SQL query that returns scalar values
    // /// </summary>
    // /// <typeparam name="T">The type of scalar result</typeparam>
    // /// <param name="sql">SQL query with parameter placeholders</param>
    // /// <param name="parameters">SQL parameters to safely inject into query</param>
    // /// <returns>List of scalar values</returns>
    // Task<List<T>> ExecuteRawScalarQueryAsync<T>(string sql, params object[] parameters);

    // /// <summary> 
    // /// Executes a non-query SQL command (INSERT, UPDATE, DELETE)
    // /// </summary>
    // /// <param name="sql">SQL command with parameter placeholders</param>
    // /// <param name="parameters">SQL parameters to safely inject into command</param>
    // /// <returns>Number of rows affected</returns>
    // Task<int> ExecuteRawNonQueryAsync(string sql, params object[] parameters);
}

