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

    public static dynamic ReadInFile(string filePath, string fileName)
    {
        Console.WriteLine($"Reading in {fileName}...");
        var streamReader = new StreamReader(filePath);
        var fileStream = streamReader.ReadToEnd();
        return JsonConvert.DeserializeObject(fileStream);
    }
}