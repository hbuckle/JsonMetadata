using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace JsonMetadata.Models
{
  [DataContract]
  public class JsonBoxSet : JsonObject
  {
    [DataMember(Name = "title", Order = 701)]
    public string title { get; set; }

    [DataMember(Name = "sorttitle", Order = 702)]
    public string sorttitle { get; set; }

    // [DataMember(Name = "dateadded", Order = 703)]
    // public DateTime dateadded { get; set; }

    [DataMember(Name = "communityrating", Order = 704)]
    public float? communityrating { get; set; }

    [DataMember(Name = "overview", Order = 705)]
    public string overview { get; set; }

    [DataMember(Name = "releasedate", Order = 706)]
    public DateTime? releasedate { get; set; }

    [DataMember(Name = "year", Order = 707)]
    public int? year { get; set; }

    [DataMember(Name = "parentalrating", Order = 708)]
    public int? parentalrating { get; set; }

    [DataMember(Name = "customrating", Order = 709)]
    public string customrating { get; set; }

    [DataMember(Name = "displayorder", Order = 710)]
    public string displayorder { get; set; }

    [DataMember(Name = "tmdbid", Order = 711)]
    public string tmdbid { get; set; }

    [DataMember(Name = "genres", Order = 712)]
    public string[] genres { get; set; }

    [DataMember(Name = "people", Order = 713)]
    public List<JsonCastCrew> people { get; set; }

    [DataMember(Name = "studios", Order = 714)]
    public string[] studios { get; set; }

    [DataMember(Name = "tags", Order = 715)]
    public string[] tags { get; set; }

    [DataMember(Name = "collectionitems", Order = 716)]
    public List<JsonObject> collectionitems { get; set; }

    [DataMember(Name = "lockdata", Order = 717)]
    public bool lockdata { get; set; }
  }
}