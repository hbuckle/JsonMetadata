using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace JsonMetadata.Models
{
  [DataContract]
  public class JsonMovie : JsonObject
  {
    [DataMember(Name = "title", Order = 1)]
    public string title { get; set; }

    [DataMember(Name = "originaltitle", Order = 2)]
    public string originaltitle { get; set; }

    [DataMember(Name = "sorttitle", Order = 3)]
    public string sorttitle { get; set; }

    // [DataMember(Name = "dateadded", Order = 4)]
    // public DateTime dateadded { get; set; }

    [DataMember(Name = "communityrating", Order = 5)]
    public float? communityrating { get; set; }

    [DataMember(Name = "criticrating", Order = 6)]
    public float? criticrating { get; set; }

    [DataMember(Name = "tagline", Order = 7)]
    public string tagline { get; set; }

    [DataMember(Name = "overview", Order = 8)]
    public string overview { get; set; }

    [DataMember(Name = "releasedate", Order = 9)]
    public DateTime? releasedate { get; set; }

    [DataMember(Name = "year", Order = 10)]
    public int? year { get; set; }

    [DataMember(Name = "parentalrating", Order = 11)]
    public int? parentalrating { get; set; }

    [DataMember(Name = "customrating", Order = 12)]
    public string customrating { get; set; }

    [DataMember(Name = "originalaspectratio", Order = 13)]
    public string originalaspectratio { get; set; }

    // [DataMember(Name = "3dformat", Order = 14)]
    // public string threedformat { get; set; }

    [DataMember(Name = "imdbid", Order = 15)]
    public string imdbid { get; set; }

    [DataMember(Name = "tmdbid", Order = 16)]
    public string tmdbid { get; set; }

    [DataMember(Name = "tmdbcollectionid", Order = 17)]
    public string tmdbcollectionid { get; set; }

    [DataMember(Name = "genres", Order = 18)]
    public List<string> genres { get; set; }

    [DataMember(Name = "people", Order = 19)]
    public List<JsonCastCrew> people { get; set; }

    [DataMember(Name = "studios", Order = 20)]
    public List<string> studios { get; set; }

    [DataMember(Name = "tags", Order = 21)]
    public List<string> tags { get; set; }

    [DataMember(Name = "lockdata", Order = 22)]
    public bool lockdata { get; set; }
  }
}