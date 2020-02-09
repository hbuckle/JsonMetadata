using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonMetadata.Models {
  public class DateTimeConverter : JsonConverter<DateTime> {
    private readonly string format;
    public DateTimeConverter(string Format) {
      this.format = Format;
    }
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      return DateTime.ParseExact(reader.GetString(), format, System.Globalization.CultureInfo.InvariantCulture);
    }
    public override void Write(Utf8JsonWriter writer, DateTime dateTimeValue, JsonSerializerOptions options) {
      writer.WriteStringValue(dateTimeValue.ToString(format, System.Globalization.CultureInfo.InvariantCulture));
    }
  }

  public class TmdbidConverter : JsonConverter<long?> {
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      if (reader.TokenType == JsonTokenType.String) {
        var s = reader.GetString();
        return long.Parse(s);
      }
      if (reader.TokenType == JsonTokenType.Null) {
        return null;
      }
      return reader.GetInt64();
    }

    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options) {
      if (value.HasValue) {
        writer.WriteNumberValue(value.Value);
      } else {
        writer.WriteNullValue();
      }
    }
  }
}
