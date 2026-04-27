
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace ChatNetworking.JsonProtocol
{
    public static class JsonFactory
    {
        // Java: private static final DateTimeFormatter FORMATTER = DateTimeFormatter.ISO_LOCAL_DATE_TIME;
        // In C#, ISO 8601 is the default, but we'll specify the format for 1:1 parity.
        private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";

        /// <summary>
        /// Creates a configured JsonSerializerOptions instance.
        /// Equivalent to Java's GsonFactory.create().
        /// </summary>
        public static JsonSerializerOptions Create()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Standard for JSON
                WriteIndented = false
            };

            // Java: .registerTypeAdapter(LocalDateTime.class, new LocalDateTimeAdapter())
            options.Converters.Add(new DateTimeConverter());

            return options;
        }

        /// <summary>
        /// Custom converter for DateTime to handle ISO_LOCAL_DATE_TIME formatting.
        /// Equivalent to Java's LocalDateTimeAdapter.
        /// </summary>
        private class DateTimeConverter : JsonConverter<DateTime>
        {
            // The compiler was complaining because it expects 'Utf8JsonReader', not 'JsonReader'
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return DateTime.MinValue;
                }

                // Use reader.GetString() to get the ISO string from the JSON stream
                string? dateStr = reader.GetString();

                if (string.IsNullOrEmpty(dateStr))
                {
                    return DateTime.MinValue;
                }

                return DateTime.ParseExact(dateStr, DateTimeFormat, CultureInfo.InvariantCulture);
            }

            // Ensure the Write method uses 'Utf8JsonWriter'
            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(DateTimeFormat));
            }
        }
    }
}