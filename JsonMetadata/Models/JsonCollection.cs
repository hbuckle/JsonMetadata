using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JsonMetadata.Models {
  public class JsonCollection : JsonObject {
    public string title { get; set; }
    public string sorttitle { get; set; }
    // public DateTime dateadded { get; set; }
    public float? communityrating { get; set; }
    public string overview { get; set; }
    public DateTime? releasedate { get; set; }
    public int? year { get; set; }
    public string parentalrating { get; set; }
    public string customrating { get; set; }
    public string displayorder { get; set; }
    [JsonConverter(typeof(TmdbidConverter))]
    public long? tmdbid { get; set; }
    public string[] genres { get; set; }
    public List<JsonCastCrew> people { get; set; } = new List<JsonCastCrew>();
    public string[] studios { get; set; }
    public string[] tags { get; set; }
    public bool lockdata { get; set; }
  }
}
