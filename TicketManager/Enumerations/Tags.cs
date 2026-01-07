using System.Text.Json.Serialization;

namespace TicketManager.Enumerations;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Tags
{
    Billing,
    Urgent,
    FeatureRequest,
    BugReport,
    SalesInquiry,
    ProductIssue,
    CustomerCare
}
