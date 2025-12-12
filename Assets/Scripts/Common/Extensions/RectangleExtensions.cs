using System.Collections.Generic;
using UnityEngine;

public static class RectangleExtensions
{
    public static bool IsIntersect(this Rectangle r1, Rectangle r2)
    {
        bool separetedOnX = r1.TopRight.x <= r2.TopLeft.x
            || r1.TopLeft.x >= r2.TopRight.x;
        bool separetedOnY = r1.TopLeft.y <= r2.BottomRight.y
            || r1.BottomRight.y >= r2.TopLeft.y;

        return separetedOnX == false && separetedOnY == false;
    }

    public static IEnumerable<Rectangle> Exclude(this Rectangle current, Rectangle other)
    {
        if (current.IsIntersect(other) == false)
        {
            yield return current;
            yield break;
        }

        float currLeft = current.TopLeft.x;
        float currTop = current.TopLeft.y;
        float currRight = current.BottomRight.x;
        float currBottom = current.BottomRight.y;

        float otherLeft = other.TopLeft.x;
        float otherTop = other.TopLeft.y;
        float otherRight = other.BottomRight.x;
        float otherBottom = other.BottomRight.y;

        float interLeft = Mathf.Max(currLeft, otherLeft);
        float interRight = Mathf.Min(currRight, otherRight);
        float interTop = Mathf.Min(currTop, otherTop);
        float interBottom = Mathf.Max(currBottom, otherBottom);

        if (currLeft < interLeft)
        {
            yield return CreateRectangle(
                currLeft, currTop,
                interLeft, currBottom
            );
        }

        if (currRight > interRight)
        {
            yield return CreateRectangle(
                interRight, currTop,
                currRight, currBottom
            );
        }

        if (currTop > interTop)
        {
            yield return CreateRectangle(
                interLeft, currTop,
                interRight, interTop
            );
        }

        if (currBottom < interBottom)
        {
            yield return CreateRectangle(
                interLeft, interBottom,
                interRight, currBottom
            );
        }
    }

    public static (float xMin, float yMin, float xMax, float yMax) CalculateMinMax(this IEnumerable<Rectangle> rectangles)
    {
        float xMin = float.MaxValue;
        float yMin = float.MaxValue;
        float xMax = float.MinValue;
        float yMax = float.MinValue;

        foreach (Rectangle rectangle in rectangles)
        {
            xMin = Mathf.Min(xMin, rectangle.BottomLeft.x);
            yMin = Mathf.Min(yMin, rectangle.BottomLeft.y);
            xMax = Mathf.Max(xMax, rectangle.TopRight.x);
            yMax = Mathf.Max(yMax, rectangle.TopRight.y);
        }

        return (xMin, yMin, xMax, yMax);
    }

    private static Rectangle CreateRectangle(float left, float top, float right, float bottom)
    {
        float actualLeft = Mathf.Min(left, right);
        float actualRight = Mathf.Max(left, right);
        float actualTop = Mathf.Max(top, bottom);
        float actualBottom = Mathf.Min(top, bottom);

        Rectangle rect = new Rectangle(
            new Vector2(actualLeft, actualTop),
            new Vector2(actualRight, actualBottom));

        return rect;
    }
}