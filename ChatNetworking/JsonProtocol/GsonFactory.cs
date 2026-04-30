using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace ChatNetworking.JsonProtocol
{
    public static class GsonFactory
    {
        // Java: private static final DateTimeFormatter FORMATTER = DateTimeFormatter.ISO_LOCAL_DATE_TIME;
        private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";

        /// <summary>
        /// Creates a configured JsonSerializerOptions instance.
        /// Equivalent to Java's GsonFactory.create().
        /// </summary>
        public static JsonSerializerOptions Create()
        {
            var options = new JsonSerializerOptions
            {
                // This makes C# properties like 'ArtistName' become 'artistName' in JSON
                //this was added for java-c#
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
                WriteIndented = false
            };

            // Handles the Java LocalDateTime parsing
            options.Converters.Add(new DateTimeConverter());
            
            // THE BRIDGE: Converts C#'s 'AddArtist' into Java's 'ADD_ARTIST'
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)); 

            return options;
        }

        /// <summary>
        /// Custom converter for DateTime to handle ISO_LOCAL_DATE_TIME formatting.
        /// Equivalent to Java's LocalDateTimeAdapter.
        /// </summary>
        private class DateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return DateTime.MinValue;
                }

                string? dateStr = reader.GetString();

                if (string.IsNullOrEmpty(dateStr))
                {
                    return DateTime.MinValue;
                }

                //this was included because Java sends time format with millisecond precision
                if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime result))
                {
                    return result;
                }
                
                return DateTime.ParseExact(dateStr, DateTimeFormat, CultureInfo.InvariantCulture);
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(DateTimeFormat));
            }
        }
    }
}