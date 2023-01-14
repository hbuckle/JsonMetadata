using System.Collections.Generic;

namespace JsonMetadata.Models {
  public class JsonObject {
    public JsonObject() { }
    public Dictionary<string, string> customfields { get; set; } = new Dictionary<string, string>();
    public long id { get; set; }
  }
}
