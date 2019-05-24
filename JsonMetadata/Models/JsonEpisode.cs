using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace JsonMetadata.Models
{
  [DataContract]
  public class JsonEpisode : JsonObject
  {
    [DataMember(Name = "title", Order = 501)]
    public string title { get; set; }

    [DataMember(Name = "sorttitle", Order = 502)]
    public string sorttitle { get; set; }

    // [DataMember(Name = "dateadded", Order = 503)]
    // public DateTime dateadded { get; set; }

    [DataMember(Name = "seasonnumber", Order = 504)]
    public int? seasonnumber { get; set; }

    [DataMember(Name = "episodenumber", Order = 505)]
    public int? episodenumber { get; set; }

    [DataMember(Name = "communityrating", Order = 506)]
    public float? communityrating { get; set; }

    [DataMember(Name = "overview", Order = 507)]
    public string overview { get; set; }

    [DataMember(Name = "releasedate", Order = 508)]
    public DateTime? releasedate { get; set; }

    [DataMember(Name = "year", Order = 509)]
    public int? year { get; set; }

    [DataMember(Name = "parentalrating", Order = 510)]
    public string parentalrating { get; set; }

    [DataMember(Name = "customrating", Order = 511)]
    public string customrating { get; set; }

    [DataMember(Name = "originalaspectratio", Order = 512)]
    public string originalaspectratio { get; set; }

    // [DataMember(Name = "3dformat", Order = 513)]
    // public string threedformat { get; set; }

    [DataMember(Name = "imdbid", Order = 514)]
    public string imdbid { get; set; }

    [DataMember(Name = "tvdbid", Order = 515)]
    public string tvdbid { get; set; }

    [DataMember(Name = "genres", Order = 516)]
    public string[] genres { get; set; }

    [DataMember(Name = "people", Order = 517)]
    public List<JsonCastCrew> people { get; set; }

    [DataMember(Name = "studios", Order = 518)]
    public string[] studios { get; set; }

    [DataMember(Name = "tags", Order = 519)]
    public string[] tags { get; set; }

    [DataMember(Name = "lockdata", Order = 520)]
    public bool lockdata { get; set; }
  }
}