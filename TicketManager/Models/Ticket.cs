using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketManager.DTOs;
using TicketManager.Services;

namespace TicketManager.Models;

public class Ticket
{
    /// <summary>
    /// Ticket undiqe identifier
    /// Generated in the database during creation
    /// </summary>
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TicketId { get; set; }

    /// <summary>
    /// Gets or sets the email of the ticket owner
    /// </summary>
    public string CreatorEmail { get; set; }

    /// <summary>
    /// Gets or sets the ticket title
    /// </summary>
    [Required]
    [Length(minimumLength: 1, maximumLength: 200, ErrorMessage = "Title should have 1 to 100 characters")]
    public string? Title { get; set; }
    /// <summary>
    /// Gets or sets the ticket's description
    /// </summary>
    [Required]
    [Length(minimumLength: 3, maximumLength: 200, ErrorMessage = "Description should have at least 3 characters and 200 characters at most")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the tickets assignee email
    /// </summary>
    public string AssigneeEmail { get; set; }

    /// <summary>
    /// Gets or sets the ticket priority
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Gets or sets the ticket tag
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets the tickets status
    /// </summary>
    public string? TicketStatus { get; set; }

    /// <summary>
    /// Gets or sets the creation time of the ticket
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the ticket promise date
    /// </summary>
    public DateTimeOffset? PromiseDate { get; set; }

    /// <summary>
    /// Gets or sets the date of the ticket update
    /// </summary>
    public DateTimeOffset UpdatedDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 
    /// </summary>
    public string? AdditionalInfo { get; set; }

    /// <summary>
    /// Gets or set ticket attachment(s)
    /// </summary>
    public List<string> Attachments { get; set; } = new List<string>();

    /// <summary>
    /// Gets or set the ticket metadata
    /// </summary>
    public List<TicketMetadata> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or set the ticket css
    /// </summary>
    public List<string>? CCS { get; set; } = new List<string>();
}
