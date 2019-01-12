using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace JsonMetadata.Models
{
  [DataContract]
  [KnownType("KnownTypes")]
  public class JsonObject
  {
    protected internal JsonObject() { }

    [DataMember(Name = "id", Order = 0)]
    public long id { get; set; }

    [DataMember(Name = "path", Order = 1)]
    public string path { get; set; }

    public static IEnumerable<Type> KnownTypes()
    {
      return from t in typeof(JsonObject).Assembly.GetTypes()
             where typeof(JsonObject).IsAssignableFrom(t)
             select t;
    }
  }
}