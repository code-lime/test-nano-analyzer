using System.Collections.Generic;
using System.Linq;

namespace NanoAnalyzer.Extremum.Part;

public class SimplePartInfo : IPartInfo
{
    public IEnumerable<(double value, bool isMinimum)> Points { get; }
    public double Minimum { get; }
    public double Maximum { get; }
    public PartType Type => PartType.None;

    public SimplePartInfo(IEnumerable<double> points)
    {
        (double value, bool isMinimum)[] _points = points.Select(v => (v, false)).ToArray();

        Minimum = _points.Min(v => v.value);
        Maximum = _points.Max(v => v.value);

        Points = _points;
    }
}
