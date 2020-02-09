using System.Text.Json.Serialization;

namespace JsonMetadata.Models {
  public class JsonCastCrew : JsonObject {
    public string name { get; set; }
    public string type { get; set; }
    [JsonConverter(typeof(TmdbidConverter))]
    public long? tmdbid { get; set; }
    public string imdbid { get; set; }
    public string role { get; set; }
  }
}
