using SixLabors.ImageSharp.PixelFormats;
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

    public Rgba32 ToPixel((double min, double delta) minDeltaX, (double min, double delta) minDeltaY)
    {
        if (x.minimum) return new Rgba32(1f, 1f, 1f);
        if (y.minimum) return new Rgba32(0f, 0f, 0f);
        return new Rgba32((float)((x.value - minDeltaX.min) / minDeltaX.delta), (float)((y.value - minDeltaY.min) / minDeltaY.delta), 0);
    }
}
