using System;
using System.Runtime.Serialization;

namespace JsonMetadata.Models
{
  [DataContract]
  public class JsonCastCrew : JsonObject
  {
    [DataMember(Name = "name", Order = 201)]
    public string name { get; set; }

    [DataMember(Name = "type", Order = 202)]
    public string type { get; set; }

    [DataMember(Name = "tmdbid", Order = 203)]
    public string tmdbid { get; set; }

    [DataMember(Name = "imdbid", Order = 204)]
    public string imdbid { get; set; }

    [DataMember(Name = "role", Order = 205)]
    public string role { get; set; }

    [DataMember(Name = "thumb", Order = 206)]
    public string thumb { get; set; }
  }
}