using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExaminationSystem
{
    public class AnswerJsonConverter : JsonConverter<Answer>
    {
        public override Answer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? s = reader.GetString();
                return new Answer(s ?? string.Empty);
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                if (doc.RootElement.TryGetProperty("Content", out var contentEl))
                {
                    return new Answer(contentEl.GetString() ?? string.Empty);
                }
                // Gracefully handle unknown object shape
                return new Answer(doc.RootElement.ToString());
            }

            if (reader.TokenType == JsonTokenType.Null)
            {
                return new Answer(string.Empty);
            }

            throw new JsonException($"Invalid JSON token for Answer: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, Answer value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.Content);
        }
    }
}
