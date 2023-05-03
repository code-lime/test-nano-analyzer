using System.IO;

namespace NanoAnalyzer.Extremum.Image;

public readonly struct HValue
{
    public readonly double value;
    public readonly bool minimum;

    public HValue(double value, bool minimum)
    {
        this.value = value;
        this.minimum = minimum;
    }

    public static HValue Read(BinaryReader reader)
    {
        return new HValue(reader.ReadDouble(), reader.ReadBoolean());
    }
    public void Write(BinaryWriter writer)
    {
        writer.Write(value);
        writer.Write(minimum);
    }
}
