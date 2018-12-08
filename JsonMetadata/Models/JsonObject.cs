using System;
using System.Runtime.Serialization;

namespace JsonMetadata.Models
{
  [DataContract]
  public class JsonObject
  {
    [DataMember(Name = "id", Order = 0)]
    public long id { get; set; }
  }
}