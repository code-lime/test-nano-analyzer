using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NanoAnalyzer;

public static class JsonUtils
{
    public static void ToJsonFile<T>(this IEnumerable<T> records, string fileName)
    {
        File.WriteAllText(fileName, JsonConvert.SerializeObject(records.ToList(), Formatting.Indented));
    }
    public static byte[] ToJson<T>(this IEnumerable<T> records)
    {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(records.ToList(), Formatting.Indented));
    }
}
