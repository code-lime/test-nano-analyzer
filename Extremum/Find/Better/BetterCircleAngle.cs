using System;
using System.Collections.Generic;

namespace NanoAnalyzer.Extremum.Find.Better;

public class BetterCircleAngle : IBetter
{
    public IEnumerable<(int x, int y)> FindBetter(Random rnd, int px, int py, bool[,] border)
    {
        int width = border.GetLength(0);
        int height = border.GetLength(1);
        (int x, int y) BetterO(int x, int y, double ox, double oy)
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

            return ((min.x + max.x) / 2, (min.y + max.y) / 2);
        }

        for (int i = 0; i < 36; i++)
        {
            int a = rnd.Next(0, 36);

            double ox = Math.Abs(Math.Cos(a * 10 * Math2D.DEG_TO_RAD));
            double oy = Math.Abs(Math.Sin(a * 10 * Math2D.DEG_TO_RAD));

            yield return BetterO(px, py, ox, oy);
        }
    }
}
