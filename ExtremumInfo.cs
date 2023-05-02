using System;
using System.Collections.Generic;
using System.Linq;

namespace NanoAnalyzer;

public interface IExtremumInfo
{
    public double Minimum { get; }
    public double Maximum { get; }
    public IEnumerable<(double value, bool isMinimum)> Points { get; }
}

public class ExtremumInfo : IExtremumInfo
{
    public bool Positive { get; }
    public IEnumerable<(double value, bool isMinimum)> Points { get; }
    public double Minimum { get; }
    public double Maximum { get; }

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

    public ExtremumInfo(bool positive, IEnumerable<double> points)
    {
        (double value, bool isMinimum)[] values = points.Select(v => (v, false)).ToArray();

        int length = values.Length;
        int delta = 5;
        int deltaLength = delta * 2 + 1;

        Func<int, bool> compareFunc = positive ? v => v >= 0 : v => v <= 0;

        if (length <= deltaLength)
        {
            int minIndex = GetIndex(values, v => v.value, compareFunc);
            values[minIndex].isMinimum = true;
        }
        else
        {
            int minIndex = GetIndex(values, v => v.value, compareFunc);
            values[minIndex].isMinimum = true;
            /*for (int i = delta; i <= length - deltaLength; i++)
            {
                int minIndex = GetIndex(values[i..(i + deltaLength)], v => v.value, compareFunc);
                values[minIndex].isMinimum = true;
            }*/
        }

        Minimum = values.Min(v => v.value);
        Maximum = values.Max(v => v.value);

        Positive = positive;
        Points = values;
    }
}

public class SimpleInfo : IExtremumInfo
{
    public IEnumerable<(double value, bool isMinimum)> Points { get; }
    public double Minimum { get; }
    public double Maximum { get; }

    public SimpleInfo(IEnumerable<double> points)
    {
        (double value, bool isMinimum)[] _points = points.Select(v => (v, false)).ToArray();

        Minimum = _points.Min(v => v.value);
        Maximum = _points.Max(v => v.value);

        Points = _points;
    }
}
