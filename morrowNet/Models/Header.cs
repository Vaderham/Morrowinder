using System.Text.Json.Serialization;

namespace morrowNet.Models;

public class Header : IEspEntry
{
    [JsonPropertyName("type")] public string type { get; set; }
    
    [JsonPropertyName("flags")] public int[] flags { get; set; }
    
    [JsonPropertyName("version")] public double version { get; set; }

    [JsonPropertyName("file_type")] public string file_type { get; set; }

    [JsonPropertyName("author")] public string author { get; set; }

    [JsonPropertyName("description")] public string description { get; set; }

    [JsonPropertyName("num_objects")] public int num_objects { get; set; }

    [JsonPropertyName("masters")] public IEnumerable<dynamic> masters { get; set; }
}