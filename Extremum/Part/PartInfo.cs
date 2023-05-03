using System;
using System.Collections.Generic;
using System.Linq;

namespace NanoAnalyzer.Extremum.Part;

public class PartInfo : IPartInfo
{
    public bool Positive { get; }
    public IEnumerable<(double value, bool isMinimum)> Points { get; }
    public double Minimum { get; }
    public double Maximum { get; }
    public PartType Type => Positive ? PartType.Positive : PartType.Negative;

    private static int GetIndex<T, TKey>(T[] array, Func<T, TKey> keyed, Func<int, bool> compareFunc)
    {
        int length = array.Length;
        if (length == 0) return -1;

        Comparer<TKey> comparer = Comparer<TKey>.Default;
        (TKey value, int index) min = (keyed.Invoke(array[0]), 0);

        for (int i = 1; i < length; i++)
        {
            TKey value = keyed.Invoke(array[i]);
            if (!compareFunc.Invoke(comparer.Compare(min.value, value))) continue;
            min = (value, i);
        }

        return min.index;
    }

    public PartInfo(bool positive, IEnumerable<double> points)
    {
        (double value, bool isMinimum)[] values = points.Select(v => (v, false)).ToArray();

        int minIndex = GetIndex(values, v => v.value, positive ? v => v >= 0 : v => v <= 0);
        values[minIndex].isMinimum = true;

        Minimum = values.Min(v => v.value);
        Maximum = values.Max(v => v.value);

        Positive = positive;
        Points = values;
    }
}
