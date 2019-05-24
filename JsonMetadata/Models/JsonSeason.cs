using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace JsonMetadata.Models
{
  [DataContract]
  public class JsonSeason : JsonObject
  {
    [DataMember(Name = "title", Order = 601)]
    public string title { get; set; }

    [DataMember(Name = "sorttitle", Order = 602)]
    public string sorttitle { get; set; }

    // [DataMember(Name = "dateadded", Order = 603)]
    // public DateTime dateadded { get; set; }

    [DataMember(Name = "seasonnumber", Order = 604)]
    public int? seasonnumber { get; set; }

    [DataMember(Name = "communityrating", Order = 605)]
    public float? communityrating { get; set; }

    [DataMember(Name = "overview", Order = 606)]
    public string overview { get; set; }

    [DataMember(Name = "releasedate", Order = 607)]
    public DateTime? releasedate { get; set; }

    [DataMember(Name = "year", Order = 608)]
    public int? year { get; set; }

    [DataMember(Name = "parentalrating", Order = 609)]
    public string parentalrating { get; set; }

    [DataMember(Name = "customrating", Order = 610)]
    public string customrating { get; set; }

    [DataMember(Name = "tvdbid", Order = 611)]
    public string tvdbid { get; set; }

    [DataMember(Name = "genres", Order = 612)]
    public string[] genres { get; set; }

    [DataMember(Name = "people", Order = 613)]
    public List<JsonCastCrew> people { get; set; }

    [DataMember(Name = "studios", Order = 614)]
    public string[] studios { get; set; }

    [DataMember(Name = "tags", Order = 615)]
    public string[] tags { get; set; }

    [DataMember(Name = "lockdata", Order = 616)]
    public bool lockdata { get; set; }
  }
}