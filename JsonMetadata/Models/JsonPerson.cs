using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace JsonMetadata.Models
{
  [DataContract]
  public class JsonPerson : JsonObject
  {
    [DataMember(Name = "name", Order = 101)]
    public String name { get; set; }

    [DataMember(Name = "overview", Order = 102)]
    public String overview { get; set; }

    [DataMember(Name = "birthdate", Order = 103)]
    public DateTime? birthdate { get; set; }

    [DataMember(Name = "birthyear", Order = 104)]
    public int? birthyear { get; set; }

    [DataMember(Name = "placeofbirth", Order = 105)]
    public String placeofbirth { get; set; }

    [DataMember(Name = "deathdate", Order = 106)]
    public DateTime? deathdate { get; set; }

    [DataMember(Name = "imdbid", Order = 107)]
    public string imdbid { get; set; }

    [DataMember(Name = "tmdbid", Order = 108)]
    public string tmdbid { get; set; }

    [DataMember(Name = "tags", Order = 109)]
    public List<string> tags { get; set; }

    [DataMember(Name = "lockdata", Order = 110)]
    public bool lockdata { get; set; }

    [DataMember(Name = "images", Order = 111)]
    public List<JsonImage> images { get; set; }
  }
}