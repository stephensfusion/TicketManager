using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TicketManager.Models;

public class TicketMetadata
{
[Key]
[Required]
[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
public int Id{get; set;}
public int TicketId {get;set;}
public string ?Key {get; set;}
public string ?Value {get;set;}

[ForeignKey("TicketId")]
[JsonIgnore]
public Ticket ?Ticket {get;set;}
}
