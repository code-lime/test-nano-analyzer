using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NanoAnalyzer;

public static class CsvUtils
{
    public static byte[] ToCsv<T>(this IEnumerable<T> records)
    {
        using MemoryStream stream = new MemoryStream();
        using StreamWriter writer = new StreamWriter(stream);
        using CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(records);
        writer.Flush();
        return stream.ToArray();
    }
    public static async Task<byte[]> ToCsvAsync<T>(this IEnumerable<T> records, CancellationToken cancellationToken = default)
    {
        using MemoryStream stream = new MemoryStream();
        using StreamWriter writer = new StreamWriter(stream);
        using CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(records, cancellationToken);
        await writer.FlushAsync();
        return stream.ToArray();
    }
    public static void ToCsvFile<T>(this IEnumerable<T> records, string fileName)
    {
        using Stream stream = File.Open(fileName, FileMode.Create);
        using StreamWriter writer = new StreamWriter(stream);
        using CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(records);
        writer.Flush();
    }
    public static async Task ToCsvFileAsync<T>(this IEnumerable<T> records, string fileName, CancellationToken cancellationToken = default)
    {
        using Stream stream = File.Open(fileName, FileMode.Create);
        using StreamWriter writer = new StreamWriter(stream);
        using CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(records, cancellationToken);
        await writer.FlushAsync();
    }
}
