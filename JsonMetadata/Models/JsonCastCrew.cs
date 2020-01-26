namespace JsonMetadata.Models {
  public class JsonCastCrew : JsonObject {
    public string name { get; set; }
    public string type { get; set; }
    public string tmdbid { get; set; }
    public string imdbid { get; set; }
    public string role { get; set; }
  }
}
