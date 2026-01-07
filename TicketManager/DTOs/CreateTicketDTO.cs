using System.ComponentModel.DataAnnotations;

namespace TicketManager.DTOs;

/// <summary>
/// Represents the data transfer object for creating a ticket
/// </summary>
public class CreateTicketDTO
{
    /// <summary>
    /// Gets or sets the email of user creating the ticket
    /// </summary>
    public string CreatorEmail { get; set; }
    /// <summary>
    /// Gets or sets the title of the ticket
    /// </summary>
    [Length(minimumLength: 1, maximumLength: 100, ErrorMessage = "Title should have 1 to 100 characters")]
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the ticket
    /// </summary>
    [Length(minimumLength: 3, maximumLength: 200, ErrorMessage = "Description should have at least 3 characters and 200 characters at most")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// Gets or sets the assignee emaail of the ticket
    /// </summary>
    public string? AssigneeEmail { get; set; }

    /// <summary>
    /// Gets or sets the ticket status 
    /// </summary>
    public string? TicketStatus { get; set; }

    /// <summary>
    /// Gets or sets the priority of the ticket
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Gets or sets the tag of the ticket
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets additional information of the ticket
    /// </summary>
    public string? AdditionalInfo { get; set; }

    /// <summary>
    /// Gets or sets the promise date of the ticket
    /// </summary>
    public DateTimeOffset? PromiseDate { get; set; } = null;

    /// <summary>
    /// Gets or sets the ticket metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Gets or set the ticket css
    /// </summary>
    public List<string>? CCS { get; set; }
}

