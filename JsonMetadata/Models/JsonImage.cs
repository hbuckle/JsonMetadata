using System;
using System.Runtime.Serialization;

namespace JsonMetadata.Models {
  [DataContract]
  public class JsonImage : JsonObject {
    [DataMember(Name = "type", Order = 301)]
    public string type { get; set; }
  }
}