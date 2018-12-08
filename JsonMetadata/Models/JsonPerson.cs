using System;
using System.Runtime.Serialization;

namespace JsonMetadata.Models
{
  [DataContract]
  public class JsonPerson : JsonObject
  {
    [DataMember(Name = "name", Order = 101)]
    public String name { get; set; }
    [DataMember(Name = "type", Order = 102)]
    public String type { get; set; }
    [DataMember(Name = "tmdbid", Order = 103)]
    public String tmdbid { get; set; }
    [DataMember(Name = "imdbid", Order = 104)]
    public String imdbid { get; set; }
    [DataMember(Name = "role", Order = 105)]
    public String role { get; set; }
    [DataMember(Name = "thumb", Order = 106)]
    public String thumb { get; set; }
  }
}