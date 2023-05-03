using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace NanoAnalyzer.Extremum.Image;

public readonly struct ElementData
{
    public readonly int x;
    public readonly int y;
    public readonly double radius;
    public readonly int weight;

    public ElementData(int x, int y, double radius, int weight)
    {
        this.x = x;
        this.y = y;
        this.radius = radius;
        this.weight = weight;
    }

    public void Draw(Pen pen, IImageProcessingContext imageContext)
    {
        imageContext.Draw(pen, new EllipsePolygon(x, y, (float)radius));
    }
}
