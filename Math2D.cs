using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NanoAnalyzer;

public static class Math2D
{
    public static double Distance((double x, double y) a, (double x, double y) b)
    {
        return Math.Sqrt(Math.Pow(b.x - a.x, 2) + Math.Pow(b.y - a.y, 2));
    }
    public static double Distance((double x, double y) point, ((double x, double y) a, (double x, double y) b) line, out (double x, double y) closest)
    {
        double dx = line.b.x - line.a.x;
        double dy = line.b.y - line.a.y;
        if ((dx == 0) && (dy == 0))
        {
            // Это точка не отрезка.
            closest = line.a;
            dx = point.x - line.a.x;
            dy = point.y - line.a.y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        // Вычислим t, который минимизирует расстояние.
        double t = ((point.x - line.a.x) * dx + (point.y - line.a.y) * dy) /
            (dx * dx + dy * dy);

        // Посмотрим, представляет ли это один из сегментов
        // конечные точки или точка в середине.
        if (t < 0)
        {
            closest = (line.a.x, line.a.y);
            dx = point.x - line.a.x;
            dy = point.y - line.a.y;
        }
        else if (t > 1)
        {
            closest = (line.b.x, line.b.y);
            dx = point.x - line.b.x;
            dy = point.y - line.b.y;
        }
        else
        {
            closest = (line.a.x + t * dx, line.a.y + t * dy);
            dx = point.x - closest.x;
            dy = point.y - closest.y;
        }

        return Math.Sqrt(dx * dx + dy * dy);
    }
    public static double DistanceWithSign((double x, double y) point, ((double x, double y) a, (double x, double y) b) line)
    {
        double a = line.b.y - line.a.y;
        double b = line.a.x - line.b.x;
        double c = -(line.a.x * a + line.a.y * b);

        return (a * point.x + b * point.y + c) / (a * a + b * 2);
        /*double a = line.b.y - line.a.y;
        double b = line.b.x - line.a.x;
        double c = line.a.y * b - line.a.x * a;

        return (a * point.x + b * point.y + c) / (a * a + b * 2);*/
    }

    private static IEnumerable<double> GetExtremumValues(IEnumerable<double> points, int radius)
    {
        LinkedList<double?> around = new LinkedList<double?>(new double?[radius * 2 + 1]);
        foreach (double? point in points.OfType<double?>().Concat(Enumerable.Repeat<double?>(null, radius)))
        {
            around.RemoveFirst();
            around.AddLast(point);

            double? _first = null;
            double? _current = null;
            double? _last = null;

            int firstCount = 0;
            int lastCount = 0;

            int i = 0;
            foreach (double? _dat in around)
            {
                if (_dat is double dat)
                {
                    if (i <= radius)
                    {
                        if (_first is null) _first = dat;
                        else _first += dat;

                        firstCount++;
                    }
                    if (i == radius) _current = dat;
                    if (i >= radius)
                    {
                        if (_last is null) _last = dat;
                        else _last += dat;

                        lastCount++;
                    }
                }
                i++;
            }

            if (_current is not double current) continue;

            if (_first is double first) first /= firstCount;
            else first = current;
            if (_last is double last) last /= lastCount;
            else last = current;

            (double x, double y) a = (-radius, first);
            (double x, double y) b = (radius, last);
            (double x, double y) c = (0, current);

            yield return Distance(a, b) * Math.Sign(DistanceWithSign(c, (a, b)));
        }
    }
    private enum ExtremumType
    {
        Positive,
        None,
        Negative
    }
    public static IEnumerable<IExtremumInfo> GetExtremums(IEnumerable<double> points, int radius)
    {
        ExtremumType infoType = ExtremumType.None;
        List<double>? info = null;
        foreach (double value in GetExtremumValues(points, radius))
        {
            ExtremumType type = value switch
            {
                double v when v > 0 => ExtremumType.Positive,
                double v when v < 0 => ExtremumType.Negative,
                _ => ExtremumType.None,
            };
            if (info is null)
            {
                info = new List<double>() { value };
                infoType = type;
                continue;
            }
            if (infoType != type)
            {
                yield return infoType switch
                {
                    ExtremumType.Positive => new ExtremumInfo(true, info),
                    ExtremumType.Negative => new ExtremumInfo(false, info),
                    _ => new SimpleInfo(info),
                };
                info = new List<double>();
                infoType = type;
            }
            info.Add(value);
        }
        if (info is null) yield break;
        yield return infoType switch
        {
            ExtremumType.Positive => new ExtremumInfo(true, info),
            ExtremumType.Negative => new ExtremumInfo(false, info),
            _ => new SimpleInfo(info),
        };
    }

    public static double TriangleSqr((double x, double y) a, (double x, double y) b, (double x, double y) c)
    {
        return ((b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y)) / 2;
    }
}
