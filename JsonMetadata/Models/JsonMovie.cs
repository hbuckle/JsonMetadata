using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace JsonMetadata.Models
{
  [DataContract]
  public class JsonMovie : JsonObject
  {
    [DataMember(Name = "title", Order = 1)]
    public String title { get; set; }
    [DataMember(Name = "sorttitle", Order = 2)]
    public String sorttitle { get; set; }
    [DataMember(Name = "overview", Order = 3)]
    public String overview { get; set; }
    [DataMember(Name = "tmdbid", Order = 4)]
    public String tmdbid { get; set; }
    [DataMember(Name = "imdbid", Order = 5)]
    public String imdbid { get; set; }
    [DataMember(Name = "year", Order = 6)]
    public int? year { get; set; }
    [DataMember(Name = "lockdata", Order = 7)]
    public bool lockdata { get; set; }
    [DataMember(Name = "watched", Order = 8)]
    public bool watched { get; set; }
    [DataMember(Name = "playcount", Order = 9)]
    public int playcount { get; set; }
    [DataMember(Name = "people", Order = 100)]
    public List<JsonPerson> people { get; set; }
  }
}