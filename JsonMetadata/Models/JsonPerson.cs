using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JsonMetadata.Models {
  public class JsonPerson : JsonObject {
    public String name { get; set; }
    public String overview { get; set; }
    public DateTime? birthdate { get; set; }
    public int? birthyear { get; set; }
    public String placeofbirth { get; set; }
    public DateTime? deathdate { get; set; }
    public string imdbid { get; set; }
    [JsonConverter(typeof(TmdbidConverter))]
    public long? tmdbid { get; set; }
    public string[] tags { get; set; }
    public bool lockdata { get; set; }
    // public List<JsonImage> images { get; set; }
  }
}
