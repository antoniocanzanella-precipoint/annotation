using NetTopologySuite.Geometries;
using System;

namespace PreciPoint.Ims.Services.Annotation.Application.Common;

public static class MathExpression
{
    /// <summary>
    /// the method calculate the circle that crosses the three points given as input.
    /// </summary>
    public static void GetCircleInformation(Point p1, Point p2, Point p3, out Point center, out double radius)
    {
        // Get the perpendicular bisector of (x1, y1) and (x2, y2).
        double x1 = (p2.X + p1.X) / 2;
        double y1 = (p2.Y + p1.Y) / 2;
        double dy1 = p2.X - p1.X;
        double dx1 = -(p2.Y - p1.Y);

        // Get the perpendicular bisector of (x2, y2) and (x3, y3).
        double x2 = (p3.X + p2.X) / 2;
        double y2 = (p3.Y + p2.Y) / 2;
        double dy2 = p3.X - p2.X;
        double dx2 = -(p3.Y - p2.Y);

        // See where the lines intersect.
        double cx = (y1 * dx1 * dx2 + x2 * dx1 * dy2 - x1 * dy1 * dx2 - y2 * dx1 * dx2)
                    / (dx1 * dy2 - dy1 * dx2);
        double cy = (cx - x1) * dy1 / dx1 + y1;
        center = new Point(cx, cy);

        double dx = cx - p1.X;
        double dy = cy - p1.Y;
        radius = Math.Sqrt(dx * dx + dy * dy);
    }
}