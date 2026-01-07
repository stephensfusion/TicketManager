
namespace TicketManager.Services;

public class TimeZoneHelper
{
 public static DateTimeOffset ToUserTimeZone(DateTimeOffset utcDateTime, string timeZoneId)
        {
            try
            {
                var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

                // Convert DateTimeOffset to DateTime, then convert time zone, then back to DateTimeOffset
                DateTime utcDateTime2 = utcDateTime.UtcDateTime;
                DateTime convertedDateTime = TimeZoneInfo.ConvertTime(utcDateTime2, TimeZoneInfo.Utc, userTimeZone);

                // Create a new DateTimeOffset with the converted DateTime and the target time zone's offset
                return new DateTimeOffset(convertedDateTime, userTimeZone.GetUtcOffset(convertedDateTime));

            }
            catch (TimeZoneNotFoundException)
            {
                return utcDateTime;
            }
        }

        public static DateTimeOffset ToUtc(DateTimeOffset localDateTime, string timeZoneId)
        {
            try
            {
                var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

                // Convert DateTimeOffset to DateTime, then convert time zone, then back to DateTimeOffset
                DateTime localDateTime2 = localDateTime.DateTime;
                DateTime convertedDateTime = TimeZoneInfo.ConvertTime(localDateTime2, userTimeZone, TimeZoneInfo.Utc);

                return new DateTimeOffset(convertedDateTime, TimeSpan.Zero);
            }
            catch (TimeZoneNotFoundException)
            {
                return localDateTime.ToUniversalTime();
            }
        }
    }

