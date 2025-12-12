using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AreaExtensions
{
    public static bool IsIntersect(this Area current, Area other)
    {
        if (current == null || other == null
            || current.IsEmpty || other.IsEmpty)
            return false;

        if (IsIntersectAreasBoundingBoxes(current, other) == false)
            return false;

        foreach (Rectangle currentRectangle in current)
        {
            foreach (Rectangle otherRectangle in other)
            {
                if (currentRectangle.IsIntersect(otherRectangle))
                    return true;
            }
        }

        return false;
    }

    public static Area Exclude(this Area current, Area other)
    {
        ThrowIf.Null(current, nameof(current));
        ThrowIf.Null(other, nameof(other));

        if (current.IsEmpty || other.IsEmpty
            || current.IsIntersect(other) == false)
            return current;

        List<Rectangle> result = new(current.Rectangles);

        foreach (Rectangle otherRectangle in other.Rectangles)
        {
            List<Rectangle> nextSet = new();

            foreach (Rectangle rectangle in result)
            {
                List<Rectangle> excluded = rectangle.Exclude(otherRectangle).ToList();

                if (excluded.Count > 0)
                    nextSet.AddRange(excluded);
            }

            result = nextSet;

            if (result.Count == 0)
                break;
        }

        return new Area(result, current.Height);
    }

    public static Vector2 MaxDimensions(this Area area)
    {
        (float xMin, float yMin, float xMax, float yMax) = CalculateMinMax(area);

        return new(xMax - xMin, yMax - yMin);
    }

    public static (float xMin, float yMin, float xMax, float yMax) CalculateMinMax(this Area area)
    {
        return area.Rectangles.CalculateMinMax();
    }

    private static bool IsIntersectAreasBoundingBoxes(Area a, Area b)
    {
        (Vector2 minA, Vector2 maxA) = CalculateAreaBoundingBox(a);
        (Vector2 minB, Vector2 maxB) = CalculateAreaBoundingBox(b);

        bool separatedOnX = maxA.x <= minB.x || minA.x >= maxB.x;
        bool separatedOnY = maxA.y <= minB.y || minA.y >= maxB.y;

        return separatedOnX == false && separatedOnY == false;
    }

    private static (Vector2 min, Vector2 max) CalculateAreaBoundingBox(Area area)
    {
        if (area.IsEmpty)
            return (Vector2.zero, Vector2.zero);

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (Rectangle rect in area)
        {
            minX = Mathf.Min(minX, rect.TopLeft.x);
            minY = Mathf.Min(minY, rect.BottomRight.y);
            maxX = Mathf.Max(maxX, rect.TopRight.x);
            maxY = Mathf.Max(maxY, rect.TopLeft.y);
        }

        return (new Vector2(minX, minY), new Vector2(maxX, maxY));
    }
}