using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TicketManager.Data;
using TicketManager.DTOs;
using TicketManager.Interfaces;
using TicketManager.Models;

namespace TicketManager.Controllers
{
    /// <summary>
    /// Base controller for ticket management operations following REST API conventions.
    /// Provides CRUD operations and ticket-specific functionality with proper HTTP verbs and resource naming.
    /// </summary>
    /// <typeparam name="TicketStatus">Enumeration type for ticket state</typeparam>
    [Route("api/v1/tickets")]
    [ApiController]
    public abstract class TicketControllerBase<TicketStatus, Priority, Tags> : ControllerBase
    where TicketStatus : struct, Enum
    where Priority : struct, Enum
    where Tags : struct, Enum
    {
        private readonly ITicketManager<TicketStatus, Priority, Tags> _tickets;

        /// <summary>
        /// Initialise a new instamce of the TicketControllerBase
        /// </summary>
        /// <param name="tickets">The ticket management service handler</param>
        /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
        public TicketControllerBase(ITicketManager<TicketStatus, Priority, Tags> tickets)
        {
            this._tickets = tickets;
        }

        #region Create Ticket
        /// <summary>
        /// Creates a new ticket in the system
        /// </summary>
        /// <param name="ticketDTO">Ticket creation data transfer object</param>
        /// <param name="files">Uploads files to the ticket</param>
        /// <returns>Created ticket details</returns>
        /// <response code="200">Ticket created successfully</response>
        [HttpPost("create")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDTO ticketDTO)
        {
            await _tickets.CreateTicketAsync(ticketDTO, new());
            return Ok("Ticket created successfully");
        }
        #endregion

        #region Get All
        /// <summary>
        /// Gets all the tickets from the system
        /// </summary>
        /// <returns>All tickets </returns>
        /// <response code="200">Returns the total ticket count</response>
        [HttpGet("all")]
        public async Task<ActionResult<Ticket>> GetAllTickets()
        {
            var tickets = await _tickets.GetAllTicketsAsync();
            return Ok(tickets);
        }
        #endregion

        #region Get By ID
        /// <summary>
        /// Retrieves a specific ticket by their unique identifier
        /// </summary>
        /// <param name="id">Ticket unique identifier</param>
        /// <returns>The ticket details</returns>
        /// <response code="200">Returns the ticket/response>
        /// <response code="404">Ticket not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicketById(int id)
        {
            var ticket = await _tickets.GetTicketByIdAsync(id);
            if(ticket == null) return NotFound();
            return Ok(ticket);
        }
        #endregion

        #region Download File
        /// <summary>
        /// Retrieves a specific file from the system
        /// </summary>
        /// <param name="filepath">Path for file download</param>
        /// <returns>The retrieved file</returns>
        /// <exception cref="IOException">Thrown when file not found</exception>
        [HttpGet("downloadfile")]
        public async Task<ActionResult> DownloadFile(int id, string fileName)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var ticket = await _tickets.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound("Ticket not found");

            string filePath = Path.Combine("UploadedFiles", $"Ticket No.{id}", fileName);
            try
            {
                FileExtensionContentTypeProvider provider = new();

                if (!provider.TryGetContentType(filePath, out string? contentType))
                {
                    contentType = "application/octet-stream";
                }

                var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

                return File(bytes, contentType, Path.GetFileName(filePath));
            }
            catch (Exception)
            {
                return BadRequest($"A file with the name {fileName} does not exist");
            }
        }
        #endregion

        #region Search By Keywords
        /// <summary>
        /// Filters a list of tickets based on  title,assignee,name and description keywords
        /// </summary>
        /// <param name="keyword">Searches for a ticket with the key word</param>
        /// <returns>Ticket filtered by keyword</returns>
        [HttpGet("search-keywords")]
        public async Task<ActionResult<Ticket>> FilteredTicketByKeyWords(string keyword)
        {
            var tickets = await _tickets.FilterTicketBy(t => t.Title.ToLower().Contains(keyword.Trim().ToLower())
            || t.Description.ToLower().Contains(keyword.Trim().ToLower())
            || t.AssigneeEmail.Trim().ToLower() == keyword.Trim().ToLower()
            || t.CreatorEmail.Trim().ToLower() == keyword.Trim().ToLower()
            || t.Tag.ToLower().Contains(keyword.Trim().ToLower()));

            try
            {
                if (tickets == null)
                    return NotFound("Ticket(s) not found");

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return NotFound($"Failed to get ticket by any of the key words {ex.Message}");
            }
        }
        #endregion

        #region Search Assignee
        /// <summary>
        /// Filters a list of tickets based on assignee
        /// </summary>
        /// <param name="assignee">Searches for a ticket with the assignee</param>
        /// <returns>Ticket filtered by assignee</returns>
        [HttpGet("search-assignee")]
        public async Task<ActionResult<Ticket>> FilteredTicketByAssignee(string assignee)
        {
            var tickets = await _tickets.FilterTicketBy(t => t.AssigneeEmail.Trim().ToLower() == assignee.Trim().ToLower());
            try
            {
                if (tickets == null)
                    return NotFound("Ticket(s) not found");

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return NotFound($"Failed to get ticket by assignee {ex.Message}");
            }
        }
        #endregion

        #region Search Title
        /// <summary>
        /// Filters a list of tickets based on title
        /// </summary>
        /// <param name="title">Searches for a ticket with the title</param>
        /// <returns>Ticket filtered by title</returns>
        [HttpGet("search-title")]
        public async Task<ActionResult<Ticket>> FilteredTicketByTitle(string title)
        {
            var tickets = await _tickets.FilterTicketBy(t => t.Title.Trim().ToLower() == title.Trim().ToLower());
            try
            {
                if (tickets == null)
                    return NotFound("Ticket(s) not found");

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return NotFound($"Failed to get ticket by title {ex.Message}");
            }
        }
        #endregion

        #region Search Tag
        /// <summary>
        /// Filters a list of tickets based on the tag
        /// </summary>
        /// <param name="tag">Searches for a ticket with the tag</param>
        /// <returns>Ticket filtered by tag</returns>
        [HttpGet("search-tag")]
        public async Task<ActionResult<Ticket>> FilteredTicketByTag(string tag)
        {
            var tickets = await _tickets.FilterTicketBy(t => t.Tag.Trim().ToLower() == tag.Trim().ToLower());
            try
            {
                if (tickets == null)
                    return NotFound("Ticket(s) not found");

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return NotFound($"Failed to get ticket by tag {ex.Message}");
            }
        }
        #endregion

        #region Search Ticket
        /// <summary>
        /// Retrieves all tickets with optional filtering and pagination.
        /// </summary>
        /// <param name="pageNumber">Page number for pagination (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 50)</param>
        /// <param name="includeMetadata">Whether to include ticket metadata (default: false)</param>
        /// <param name="priority">Optional priority filter</param>
        /// <param name="status">Optional status filter</param>
        /// <param name="search">Optional search term for name/title</param>
        /// <param name="sortBy">Optional sort term for name/title</param>
        /// <returns>Paginated list of tickets</returns>
        /// <response code="200">Returns the list of tickets</response>
        /// <response code="400">Invalid pagination parameters</response>
        [HttpGet("search")]
        public async Task<IActionResult> Tickets(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] bool includeMetadata = false,
            [FromQuery] string? priority = null,
            [FromQuery] string? status = null,
            [FromQuery] string? tag = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "Name",
            [FromQuery] SearchMode searchMode = SearchMode.Contains)
        {
            // Use SearchTicketsAsync for unified filtering/pagination logic
            var (tickets, totalCount) = await _tickets.SearchTicketsAsync(
                search, priority, status, tag, sortBy, true, pageNumber, pageSize, searchMode, includeMetadata);

            var response = new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Tickets = tickets
            };
            return Ok(response);
        }
        #endregion

        // #region Advanced Search
        // /// <summary>
        // /// Performs advanced search on tickets with parameterized SQL query.
        // /// </summary>
        // /// <param name="name">Name search pattern</param>
        // /// <param name="assignee">Assignee search pattern</param>
        // /// <param name="title">Title search pattern</param>
        // /// <returns>List of matching tickets</returns>
        // /// <response code="200">Returns matching tickets</response>
        // /// <response code="400">Invalid search parameters</response>
        // [HttpGet("advanced-search")]
        // public async Task<ActionResult> AdvancedTicketSearch(
        //     [FromQuery] string name,
        //     [FromQuery] string title,
        //     [FromQuery] string assignee)

        // {
        //     if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(title))
        //         return BadRequest("At least one search parameter is required.");

        //     try
        //     {
        //         // Using parameterized queries to prevent SQL injection
        //         // builder.Services.AddDbContext<AppDbContext>();// Add DbContext in Program.cs

        //         //var db = new DatabaseFacade(dbContext);

        //         string sql =
        //             "SELECT * " +
        //             "FROM Tickets " +
        //             "WHERE Name     LIKE @name " +
        //             "  AND Title    LIKE @title " +
        //             "  AND Assignee LIKE @assignee";


        //         var tickets = await _tickets.ExecuteRawQueryAsync<Ticket>(
        //                  sql, $"%{name}%", $"%{title}%", $"%{assignee}%");

        //         return Ok(tickets);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest($"Search failed: {ex.Message}");
        //     }
        // }
        // #endregion

        #region Get Ticket By Status
        /// <summary>
        /// Gets all tickets with the same status
        /// </summary>
        /// <param name="status">Ticket status</param>
        /// <param name="includeMetadata">Incude ticket metadata</param>
        /// <returns>Updated ticket by status</returns>
        [HttpGet("status")]
        public async Task<IActionResult> GetTicketByStatus(TicketStatus status, [FromQuery] bool includeMetadata = false)
        {
            try
            {
                var tickets = await _tickets.GetTicketByStatusAsync(status, includeMetadata);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to get of tickets by status", ex);
            }
        }
        #endregion

        #region Get Ticket By Priority
        /// <summary>
        /// Gets all tickets with the same priority
        /// </summary>
        /// <param name="priority">Ticket priority</param>
        /// <param name="includeMetadata">Incude ticket metadata</param>
        /// <returns>Updated ticket by priority</returns>
        [HttpGet("priority")]
        public async Task<IActionResult> GetTicketByPriority(Priority priority, [FromQuery] bool includeMetadata = false)
        {
            try
            {
                var tickets = await _tickets.GetTicketByPriorityAsync(priority, includeMetadata);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to get of tickets by priority", ex);
            }
        }
        #endregion

        #region Get Ticket By Tag
        /// <summary>
        /// Gets all tickets with the same tag
        /// </summary>
        /// <param name="tag">Ticket tag</param>
        /// <param name="includeMetadata">Incude ticket metadata</param>
        /// <returns>Updated ticket by tag</returns>
        [HttpGet("tag")]
        public async Task<IActionResult> GetTicketByTag(Tags tag, [FromQuery] bool includeMetadata = false)
        {
            try
            {
                var tickets = await _tickets.GetTicketByTagAsync(tag, includeMetadata);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to get of tickets by tag", ex);
            }
        }
        #endregion

        #region Get Ticket By Assignee
        /// <summary>
        /// Filters a list of tickets based on assignee.
        /// </summary>
        /// <param name="assignee">Ticket assignee</param>
        /// <returns>Ticket filtered by assignee</returns>
        [HttpGet("assignee")]
        public async Task<ActionResult<Ticket>> FilteredTicketByAsignee(string assignee)
        {
            var tickets = await _tickets.FilterTicketBy(t => t.AssigneeEmail.Trim().ToLower() == assignee.ToLower());
            try
            {
                if (tickets == null)
                    return NotFound("Ticket(s) not found");

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return NotFound($"Failed to get ticket by assignee {ex.Message}");
            }
        }
        #endregion

        #region Get Ticket By Specific Date
        /// <summary>
        /// Retrieve only tickets of a specified date
        /// </summary>
        /// <param name="specific_date">Specific promise date</param>
        /// <returns>Tickets for a specific date</returns>
        [HttpGet("date")]
        public async Task<ActionResult<Ticket>> GetFilteredByDate([FromQuery] DateTimeOffset specificDate)
        {
            var tickets = await _tickets.FilterTicketBy(t => t.PromiseDate.HasValue && t.PromiseDate.Value.Date == specificDate.Date);
            try
            {
                if (tickets == null)
                    return NotFound("Ticket(s) not found");

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to get ticket by specific date,{ex.Message}");
            }
        }
        #endregion

        #region Get By Date Range
        /// <summary>
        /// Retrieves tickets for specific date range.
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date date</param>
        /// <returns>Tickets for a specific range of date</returns>
        [HttpGet("date-range")]
        public async Task<ActionResult<Ticket>> GetFilteredByDateRange([FromQuery] DateTimeOffset startDate, [FromQuery] DateTimeOffset endDate)
        {
            try
            {
                var tickets = await _tickets.FilterTicketBy(t =>
                    t.PromiseDate.HasValue &&
                    t.PromiseDate.Value.Date >= startDate.Date &&
                    t.PromiseDate.Value.Date <= endDate.Date);
                if (tickets == null)
                    return NotFound("Ticket(s) not found ");

                return Ok(tickets);
            }
            catch (Exception)
            {
                return BadRequest($"Failed to get ticket by date range");
            }
        }
        #endregion

        #region Get Number Of Tickets
        /// <summary>
        /// Gets the total ticket count
        /// </summary>
        /// <returns>Total ticket count</returns>
        [HttpGet("ticket-count")]
        public async Task<ActionResult<int>> GetNumberOfTickets()
        {
            try
            {
                return await _tickets.GetNumberOfTicketsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to get number of tickets", ex);
            }
        }
        #endregion

        #region Count by priority
        /// <summary>
        /// Gets the number of tickets with the same priority
        /// </summary>
        /// <param name="priority">Ticket priority</param>
        /// <returns>Ticket count with the same priority</returns>
        [HttpGet("count-by-priority")]
        public async Task<ActionResult<int>> GetNumberOfTicketByPriority([FromQuery] Priority priority)
        {
            try
            {
                return await _tickets.GetNumberOfTicketsByPriorityAsync(priority);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to get number of tickets by priority", ex);
            }
        }
        #endregion

        #region Count by status
        /// <summary>
        /// Gets the number of tickets with the same status
        /// </summary>
        /// <param name="status">Ticket status</param>
        /// <returns>Ticket count with the same status</returns>
        [HttpGet("count-by-status")]
        public async Task<ActionResult<int>> GetNumberOfTicketByStatus([FromQuery] TicketStatus status)
        {
            try
            {
                return await _tickets.GetNumberOfTicketsByStatusAsync(status);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to get number of tickets by status", ex);
            }
        }
        #endregion

        #region Count by tag
        /// <summary>
        /// Gets the number of tickets with the same tags
        /// </summary>
        /// <param name="tag">Ticket tag</param>
        /// <returns>Ticket count with the same tag</returns>
        [HttpGet("count-by-tag")]
        public async Task<ActionResult<int>> GetNumberOfTicketByTag([FromQuery] Tags tag)
        {
            try
            {
                return await _tickets.GetNumberOfTicketsByTagAsync(tag);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to get number of tickets by tag", ex);
            }
        }
        #endregion

        #region Update Ticket
        /// <summary>
        /// Updates a ticket by it's unique identifier
        /// </summary>
        /// <param name="id">Ticket unique identifier</param>
        /// <param name="ticketsDTO">Ticket update data transfer object</param>
        /// <returns>Updated ticket</returns>
        [HttpPatch("{id}ticket")]
        public async Task<ActionResult> UpdateTicketById(int id, [FromBody] UpdateTicketDTO ticketsDTO)
        {
            // try
            // {
            var ticket = await _tickets.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound();

            await _tickets.UpdateTicketAsync(id, ticketsDTO);
            return Ok("Ticket updated successfully");
            // }
            // catch (Exception)
            // {
            //     return BadRequest($"Failed to update ticket");
            // }
        }
        #endregion

        #region Update Attachment
        /// <summary>
        /// Updates ticket attachment by overwriting all attachments available
        /// </summary>
        /// <param name="ticketId">Ticket unique identifier</param>
        /// <param name="files">Updated file attachment</param>
        /// <returns></returns>
        [HttpPatch("overwrite-attachment(s)")]
        public async Task<IActionResult> UpdateTicketByRemovingAttachment(int ticketId, List<IFormFile> files, string fileName)
        {
            var ticket = await _tickets.GetTicketByIdAsync(ticketId);

            if (ticket == null)
                return BadRequest();

            await _tickets.UpdateTicketAttachmentAsync(ticketId, files, fileName);
            return Ok("Ticket attachment updated successfully");
        }

        /// <summary>
        /// Updates ticket attachment by adding to existing attachments
        /// </summary>
        /// <param name="ticketId">Ticket unique identifier</param>
        /// <param name="files">Updated file attachment</param>
        /// <returns></returns>
        [HttpPatch("add-attachment(s)")]
        public async Task<IActionResult> UpdateTicketAttachment(int ticketId, List<IFormFile> files)
        {
            var ticket = await _tickets.GetTicketByIdAsync(ticketId);

            if (ticket == null)
                return BadRequest();

            await _tickets.UpdateTicketAttachmentAsync(ticketId, files);
            return Ok("Ticket attachment updated successfully");
        }
        #endregion

        #region Update Title
        /// <summary>
        /// Updates a ticket title by it's unique identifier
        /// </summary>
        /// <param name="id">Ticket unique identifier</param>
        /// <param name="ticketsDTO">Ticket update data transfer object</param>
        /// <returns>Updated ticket title</returns>
        [HttpPatch("{id}title")]
        public async Task<ActionResult> UpdateTicketByTitle(int id, [FromBody] UpdateTitleDTO ticketsDTO)
        {
            var ticket = await _tickets.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound();

            if (ticketsDTO.Title != null)
            {
                ticket.Title = ticketsDTO.Title;
            }

            await _tickets.UpdateTicketTitleAsync(id, ticketsDTO);
            return Ok("Ticket title updated successfully");
        }
        #endregion

        #region Update Assignee
        /// <summary>
        /// Updates a ticket assignee by it's unique identifier
        /// </summary>
        /// <param name="id">Ticket unique identifier</param>
        /// <param name="ticketsDTO">Ticket update data transfer object</param>
        /// <returns>Updated ticket assignee</returns>
        [HttpPatch("{id}assignee")]
        public async Task<ActionResult> UpdateTicketByAssignee(int id, [FromBody] UpdateAssigneeDTO ticketsDTO)
        {
            var ticket = await _tickets.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound();

            if (ticketsDTO.AssigneeEmail != null)
            {
                ticket.AssigneeEmail = ticketsDTO.AssigneeEmail;
            }

            await _tickets.UpdateTicketAssigneeAsync(id, ticketsDTO);
            return Ok("Ticket assignee updated successfully");
        }

        #endregion

        #region Update Description
        /// <summary>
        /// Updates a ticket description by it's unique identifier
        /// </summary>
        /// <param name="id">Ticket unique identifier</param>
        /// <param name="ticketsDTO">Ticket update data transfer object</param>
        /// <returns>Updated ticket description</returns>
        [HttpPatch("{id}description")]
        public async Task<ActionResult> UpdateTicketByDescription(int id, [FromBody] UpdateDescriptionDTO ticketsDTO)
        {
            var ticket = await _tickets.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound();

            if (ticketsDTO.Description != null)
            {
                ticket.Description = ticketsDTO.Description;
            }

            await _tickets.UpdateTicketDescriptionAsync(id, ticketsDTO);
            return Ok("Ticket description updated successfully");
        }
        #endregion

        #region Update Status
        /// <summary>
        /// Updates a ticket status by it's unique identifier
        /// </summary>
        /// <param name="id">Ticket unique identifier</param>
        /// <param name="ticketsDTO">Ticket update data transfer object</param>
        /// <returns>Updated ticket status</returns>
        [HttpPatch("{id}status")]
        public async Task<IActionResult> UpdateTicketByStatus(int id, [FromBody] string status)
        {
            try
            {
                if (!Enum.TryParse<TicketStatus>(status, true, out var parsedStatus))
                    return BadRequest($"Invalid ticket status {status}");

                await _tickets.UpdateTicketStatusAsync(id, status);
                return Ok("Ticket status updated successfully");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
        #endregion

        #region Update Priority
        /// <summary>
        /// Updates a ticket priority by it's unique identifier
        /// </summary>
        /// <param name="id">Ticket unique identifier</param>
        /// <param name="ticketsDTO">Ticket update data transfer object</param>
        /// <returns>Updated ticket priority</returns>
        [HttpPatch("{id}priority")]
        public async Task<IActionResult> UpdateTicketByPriority(int id, [FromBody] string priority)
        {
            try
            {
                if (!Enum.TryParse<Priority>(priority, true, out var parsedPriority))
                    return BadRequest($"Invalid ticket priority {priority}");

                await _tickets.UpdateTicketPriorityAsync(id, priority);
                return Ok("Ticket priority updated successfully");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
        #endregion

        #region Update Tag
        /// <summary>
        /// Updates a ticket tag by it's unique identifier
        /// </summary>
        /// <param name="id">Ticket unique identifier</param>
        /// <param name="ticketsDTO">Ticket update data transfer object</param>
        /// <returns>Updated ticket tag</returns>
        [HttpPatch("{id}tag")]
        public async Task<IActionResult> UpdateTicketByTag(int id, [FromBody] string tag)
        {
            try
            {
                if (!Enum.TryParse<Tags>(tag, true, out var parsedTag))
                    return BadRequest($"Invalid ticket tag {tag}");

                await _tickets.UpdateTicketTagAsync(id, tag);
                return Ok("Ticket tag updated successfully");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
        #endregion

        #region Delete Ticket
        /// <summary>
        /// Deletes ticket using unique identifier
        /// </summary>
        /// <param name="id">Ticket unique identifier</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}ticket")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var deleteTicket = await _tickets.GetTicketByIdAsync(id);

            if (deleteTicket == null)
                return BadRequest();

            await _tickets.DeleteTicketAsync(id);
            return NoContent();
        }
        #endregion

        #region Delete Attachment
        /// <summary>
        /// Deletes an attachment from a ticket
        /// </summary>
        /// <param name="ticketId">Ticket unique identifier</param>
        /// <param name="fileName">Name of file to be deleted</param>
        /// <returns>Ticket without the deleted attachment</returns>
        [HttpDelete("attachment")]
        public async Task<IActionResult> DeleteTicketAttachment(int ticketId, string fileName)
        {
            var ticketToUpdate = await _tickets.GetTicketByIdAsync(ticketId);

            if (ticketToUpdate == null)
                return BadRequest();

            await _tickets.DeleteAttachmentAsync(ticketId, fileName);

            return Ok("Ticket attachment deleted successfully");
        }
        #endregion
    }
}
