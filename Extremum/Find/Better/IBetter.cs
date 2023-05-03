using System.Collections.Generic;
using System;

namespace NanoAnalyzer.Extremum.Find.Better;

public interface IBetter
{
    IEnumerable<(int x, int y)> FindBetter(Random rnd, int px, int py, bool[,] border);
}
