using System.Text.Json.Serialization;

namespace TicketManager.Enumerations;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Priority
{
    Critical,
    High,
    Medium,
    Low
}
