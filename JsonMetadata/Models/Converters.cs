using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonMetadata.Models {
  public class DateTimeConverter : JsonConverter<DateTime> {
    private readonly string format;
    public DateTimeConverter(string Format) {
      this.format = Format;
    }
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
            DateTime.ParseExact(reader.GetString(),
                format, System.Globalization.CultureInfo.InvariantCulture);

    public override void Write(
        Utf8JsonWriter writer,
        DateTime dateTimeValue,
        JsonSerializerOptions options) =>
            writer.WriteStringValue(dateTimeValue.ToString(
                format, System.Globalization.CultureInfo.InvariantCulture));
  }
}
