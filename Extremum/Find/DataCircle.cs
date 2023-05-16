using System;
using System.Collections.Generic;
using System.Linq;

namespace NanoAnalyzer.Extremum.Find;

public static class DataCircle
{
    public static IEnumerable<double> FindRadiusCircle(int px, int py, bool[,] border)
    {
        int width = border.GetLength(0);
        int height = border.GetLength(1);
        double BetterR(int x, int y, double ox, double oy)
        {
            (int x, int y) min = (x, y);
            for ((double _x, double _y) = (x, y); _x >= 0 && _y >= 0; _x -= ox, _y -= oy)
            {
                int __x = (int)_x;
                int __y = (int)_y;

                if (border[__x, __y])
                {
                    min = (__x, __y);
                    break;
                }
            }

            (int x, int y) max = (x, y);
            for ((double _x, double _y) = (x, y); _x < width && _y < height; _x += ox, _y += oy)
            {
                int __x = (int)_x;
                int __y = (int)_y;

                if (border[__x, __y])
                {
                    max = (__x, __y);
                    break;
                }
            }

            return Math2D.Distance(min, max);
        }

        int x = px;
        int y = py;

        for (int i = 0; i < 18; i++)
        {
            double ox = Math.Abs(Math.Cos(i * 10 * Math2D.DEG_TO_RAD));
            double oy = Math.Abs(Math.Sin(i * 10 * Math2D.DEG_TO_RAD));

            yield return BetterR(x, y, ox, oy);
        }
    }
    public static (int x, int y, double radius) FindDataCircle(int px, int py, bool[,] border)
    {
        double radius = 0;
        int count = 0;
        (int x, int y) center = (px, py);

        List<(double value, (double x, double y) a, (double x, double y) o)> radiuses = new List<(double value, (double x, double y) a, (double x, double y) o)>();

        foreach (((int x, int y) a, (double x, double y) o) in GetPointsWithOffsets(px, py, border))
        {
            count++;
            double rValue = Math2D.Distance(a, center);
            radiuses.Add((rValue, a, o));
            radius += rValue;
        }

        if (count == 0) return (px, py, 0);
        radius /= count;
        if (radius == 0) return (px, py, 0);

        double sqrUP = 0;
        _ = radiuses.Aggregate((a, b) =>
        {
            sqrUP += Math2D.TriangleSqr(
                a.value <= radius ? (a.o.x * radius + px, a.o.y * radius + py) : a.a,
                b.value <= radius ? (b.o.x * radius + px, b.o.y * radius + py) : b.a,
                center
            );
            return b;
        });
        double sqrDOWN = 0;
        _ = radiuses.Aggregate((a, b) =>
        {
            sqrDOWN += Math2D.TriangleSqr(
                a.value >= radius ? (a.o.x * radius + px, a.o.y * radius + py) : a.a,
                b.value >= radius ? (b.o.x * radius + px, b.o.y * radius + py) : b.a,
                center
            );
            return b;
        });
        double sqr = 2 * Math.PI * radius * radius;
        //if (sqrUP / sqr > 0.1 || sqrDOWN / sqr > 0.1) return (px, py, 0);
        return (center.x, center.y, radius);
    }
    private static ((int x, int y) a, (int x, int y) b) BetterR(int x, int y, double ox, double oy, bool[,] border)
    {
        int width = border.GetLength(0);
        int height = border.GetLength(1);
        bool wait = false;
        (int x, int y) min = (x, y);

        for ((double _x, double _y) = (x, y); _x >= 0 && _y >= 0 && _x < width && _y < height; _x -= ox, _y -= oy)
        {
            int __x = (int)_x;
            int __y = (int)_y;

            if (border[__x, __y])
            {
                wait = true;
                min = (__x, __y);
            }
            else if (wait)
            {
                min = (__x, __y);
                break;
            }
        }


        wait = false;
        (int x, int y) max = (x, y);

        for ((double _x, double _y) = (x, y); _x >= 0 && _y >= 0 && _x < width && _y < height; _x += ox, _y += oy)
        {
            int __x = (int)_x;
            int __y = (int)_y;

            if (border[__x, __y])
            {
                wait = true;
                max = (__x, __y);
            }
            else if (wait)
            {
                max = (__x, __y);
                break;
            }
        }

        return (min, max);
    }
    public static IEnumerable<((int x, int y) a, (int x, int y) b)> FindPointsCircle(int px, int py, bool[,] border)
    {
        int x = px;
        int y = py;

        for (int i = 0; i < 18; i++)
        {
            double ox = Math.Cos(i * 10 * Math2D.DEG_TO_RAD);
            double oy = Math.Sin(i * 10 * Math2D.DEG_TO_RAD);

            yield return BetterR(x, y, ox, oy, border);
        }
    }
    public static IEnumerable<((int x, int y) border, (double ox, double oy) circle)> GetPointsWithOffsets(int px, int py, bool[,] border)
    {
        int x = px;
        int y = py;

        List<((int x, int y) border, (double ox, double oy) circle)> aValues = new List<((int x, int y) border, (double ox, double oy) circle)>();
        List<((int x, int y) border, (double ox, double oy) circle)> bValues = new List<((int x, int y) border, (double ox, double oy) circle)>();

        (int x, int y) center = (x, y);

        for (int i = 0; i < 18; i++)
        {
            double ox = Math.Cos(i * 10 * Math2D.DEG_TO_RAD);
            double oy = Math.Sin(i * 10 * Math2D.DEG_TO_RAD);

            ((int x, int y) a, (int x, int y) b) = BetterR(x, y, ox, oy, border);

            if (a == center || b == center) return Enumerable.Empty<((int x, int y) border, (double ox, double oy) circle)>();

            aValues.Add((a, (ox, oy)));
            bValues.Add((b, (-ox, -oy)));
        }
        return aValues.Concat(bValues);
    }
}
