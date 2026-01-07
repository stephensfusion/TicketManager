namespace TicketManager.DTOs;

/// <summary>
/// Represents the data transfer object for updating a ticket
/// </summary>
public class UpdateTicketDTO
{
    /// <summary>
    /// /Gets or sets the title of the ticket
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// /Gets or sets the name of the ticket
    /// </summary>
    public string? CreatorEmail { get; set; }

    /// <summary>
    /// /Gets or sets the tag of the ticket
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// /Gets or sets additional information of the ticket
    /// </summary>
    public string? AdditionalInfo { get; set; }

    /// <summary>
    /// /Gets or sets the priority of the ticket
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// /Gets or sets the description of the ticket
    /// 
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// /Gets or sets the assignee to the ticket
    /// </summary>
    public string? AssigneeEmail { get; set; }

    /// <summary>
    /// /Gets or sets the ticket status 
    /// </summary>
    public string? TicketStatus { get; set; }

    /// <summary>
    /// Gets or sets the promise date of the ticket
    /// </summary>
    public DateTimeOffset? PromiseDate { get; set; }

    /// <summary>  
    /// Gets or sets the metadata associated with the ticket 
    /// </summary>  
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Gets or set the ticket css
    /// </summary>
    public List<string>? CCS { get; set; }
}


