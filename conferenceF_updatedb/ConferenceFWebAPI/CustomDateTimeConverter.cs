using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Lấy thời gian UTC
        DateTime utcDateTime = value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value;

        // Chuyển đổi sang múi giờ Việt Nam
        TimeZoneInfo vietnamTimeZone;
        try
        {
            vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Windows
        }
        catch (TimeZoneNotFoundException)
        {
            vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); // Linux/macOS
        }
        catch (Exception)
        {
            vietnamTimeZone = TimeZoneInfo.Utc; // Fallback
        }

        DateTime vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, vietnamTimeZone);

        // Ghi ra JSON với định dạng mong muốn
        writer.WriteStringValue(vietnamTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
    }
}