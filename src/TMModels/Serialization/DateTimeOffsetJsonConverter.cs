using System.Text.Json;
using System.Text.Json.Serialization;

namespace TMModels.Serialization;

internal class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64());

    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset dateTimeValue,
        JsonSerializerOptions options) =>
        writer.WriteNumberValue(dateTimeValue.ToUnixTimeSeconds());
}