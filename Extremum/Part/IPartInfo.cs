using System.Collections.Generic;

namespace NanoAnalyzer.Extremum.Part;

public interface IPartInfo
{
    public PartType Type { get; }
    public double Minimum { get; }
    public double Maximum { get; }
    public IEnumerable<(double value, bool isMinimum)> Points { get; }
}
