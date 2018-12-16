using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace JsonMetadata.Models
{
  [DataContract]
  public class JsonSeries : JsonObject
  {
    [DataMember(Name = "title", Order = 401)]
    public string title { get; set; }

    [DataMember(Name = "originaltitle", Order = 402)]
    public string originaltitle { get; set; }

    [DataMember(Name = "sorttitle", Order = 403)]
    public string sorttitle { get; set; }

    // [DataMember(Name = "dateadded", Order = 404)]
    // public DateTime dateadded { get; set; }

    [DataMember(Name = "status", Order = 405)]
    public string status { get; set; }

    [DataMember(Name = "communityrating", Order = 406)]
    public float? communityrating { get; set; }

    [DataMember(Name = "overview", Order = 407)]
    public string overview { get; set; }

    [DataMember(Name = "releasedate", Order = 408)]
    public DateTime? releasedate { get; set; }

    [DataMember(Name = "year", Order = 409)]
    public int? year { get; set; }

    [DataMember(Name = "enddate", Order = 410)]
    public DateTime? enddate { get; set; }

    [DataMember(Name = "airdays", Order = 411)]
    public List<string> airdays { get; set; }

    [DataMember(Name = "airtime", Order = 412)]
    public string airtime { get; set; }

    [DataMember(Name = "runtime", Order = 413)]
    public double? runtime { get; set; }

    [DataMember(Name = "parentalrating", Order = 414)]
    public int? parentalrating { get; set; }

    [DataMember(Name = "customrating", Order = 415)]
    public string customrating { get; set; }

    [DataMember(Name = "displayorder", Order = 416)]
    public string displayorder { get; set; }

    [DataMember(Name = "imdbid", Order = 417)]
    public string imdbid { get; set; }

    [DataMember(Name = "tmdbid", Order = 418)]
    public string tmdbid { get; set; }

    [DataMember(Name = "tvdbid", Order = 419)]
    public string tvdbid { get; set; }

    [DataMember(Name = "zap2itid", Order = 420)]
    public string zap2itid { get; set; }

    [DataMember(Name = "genres", Order = 421)]
    public string[] genres { get; set; }

    [DataMember(Name = "people", Order = 422)]
    public List<JsonCastCrew> people { get; set; }

    [DataMember(Name = "studios", Order = 423)]
    public string[] studios { get; set; }

    [DataMember(Name = "tags", Order = 424)]
    public string[] tags { get; set; }

    [DataMember(Name = "lockdata", Order = 425)]
    public bool lockdata { get; set; }
  }
}