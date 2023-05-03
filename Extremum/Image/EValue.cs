using System.IO;

namespace NanoAnalyzer.Extremum.Image;

public readonly struct EValue
{
    public readonly HValue x;
    public readonly HValue y;

    public EValue(HValue x, HValue y)
    {
        this.x = x;
        this.y = y;
    }

    public EValue ChangeX(HValue x) => new EValue(x, y);
    public EValue ChangeY(HValue y) => new EValue(x, y);

    public static EValue Read(BinaryReader reader)
    {
        return new EValue(HValue.Read(reader), HValue.Read(reader));
    }
    public void Write(BinaryWriter writer)
    {
        x.Write(writer);
        y.Write(writer);
    }
}
