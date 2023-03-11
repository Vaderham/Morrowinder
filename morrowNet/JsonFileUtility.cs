using Newtonsoft.Json;

namespace morrowNet;

public static class JsonFileUtils
{
    private static readonly JsonSerializerSettings _options
        = new() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented};
    
    public static void SimpleWrite(object obj, string fileName)
    {
        var jsonString = JsonConvert.SerializeObject(obj, _options);
        string deescaped = JsonConvert.DeserializeObject<string>(jsonString);
        File.WriteAllText(fileName, deescaped);
    }
}