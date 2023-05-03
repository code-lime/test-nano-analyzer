using System;

namespace NanoAnalyzer;

public static class Math2D
{
    public const double RAD_TO_DEG = 180 / Math.PI;
    public const double DEG_TO_RAD = Math.PI / 180;

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
    public static double TriangleSqr((double x, double y) a, (double x, double y) b, (double x, double y) c)
    {
        return ((b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y)) / 2;
    }
}
