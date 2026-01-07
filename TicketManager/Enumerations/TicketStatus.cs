using System.Text.Json.Serialization;

namespace TicketManager.Enumerations;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TicketStatus
{
    Open,
    InProgress,
    InReview,
    OnHold,
    Pending,
    Closed
}
