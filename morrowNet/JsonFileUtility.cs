using Newtonsoft.Json;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace morrowNet;

public static class JsonFileUtils
{
    private static readonly JsonSerializerSettings _options
        = new() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

    public static void SimpleWrite(object obj, string fileName)
    {
        var jsonString = JsonConvert.SerializeObject(obj, _options);
        var deescaped = JsonConvert.DeserializeObject<string>(jsonString);
        File.WriteAllText(fileName, deescaped);
    }

    public static void BetterWrite(object jsonobject, string filepath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(jsonobject); 
        
        File.WriteAllText(filepath, json);
    }
}