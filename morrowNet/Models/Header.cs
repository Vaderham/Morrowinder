using System.Text.Json.Serialization;

namespace morrowNet.Models;

public class Header : IEspEntry
{
    [JsonPropertyName("type")] public string type { get; set; }
    
    [JsonPropertyName("flags")] public int[] Flags { get; set; }
    
    [JsonPropertyName("version")] public double Version { get; set; }

    [JsonPropertyName("file_type")] public string FileType { get; set; }

    [JsonPropertyName("author")] public string Author { get; set; }

    [JsonPropertyName("description")] public string Description { get; set; }

    [JsonPropertyName("num_objects")] public int NumObjects { get; set; }

    [JsonPropertyName("masters")] public IEnumerable<dynamic> Masters { get; set; }
}