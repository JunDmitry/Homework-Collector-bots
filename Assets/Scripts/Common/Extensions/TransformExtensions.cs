using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    public static IEnumerator DOSequenceScaleCompletion(this Transform target, Vector3 startScale, Vector3 endScale, float duration)
    {
        bool xIsEquals = Mathf.Approximately(startScale.x, endScale.x);
        bool yIsEquals = Mathf.Approximately(startScale.y, endScale.y);
        bool zIsEquals = Mathf.Approximately(startScale.z, endScale.z);
        int partsCount = InvertConvertBoolToInt(xIsEquals) + InvertConvertBoolToInt(yIsEquals) + InvertConvertBoolToInt(zIsEquals);

        if (partsCount == 0)
            yield break;

        Sequence tween = DOTween.Sequence();

        if (xIsEquals == false)
            tween.Append(target.DOScaleX(endScale.x, duration / partsCount).From(startScale.x));

        if (yIsEquals == false)
            tween.Append(target.DOScaleY(endScale.y, duration / partsCount).From(startScale.y));

        if (zIsEquals == false)
            tween.Append(target.DOScaleZ(endScale.z, duration / partsCount).From(startScale.z));

        yield return tween.WaitForCompletion();
    }

    private static int InvertConvertBoolToInt(bool value)
    {
        return value ? 0 : 1;
    }
}

public static class VectorExtensions
{
    private static readonly int[][] s_directions = new int[][]
    {
        new int[] { 0, 1 }, new int[] { 1, 0 },
        new int[] { 0, -1 }, new int[] { -1, 0 },
        new int[] { 1, 1 }, new int[] { -1, 1 },
        new int[] { 1, -1 }, new int[] { -1, -1 }
    };

    private static Vector2Int s_verticalHorizontalRange = new(0, 4);
    private static Vector2Int s_diagonalRange = new(4, 8);

    public static IEnumerable<Vector2> GetDiagonals(this Vector2 original)
    {
        return GetInRange(original, s_diagonalRange);
    }

    public static IEnumerable<Vector2> GetVerticalAndHorizontals(this Vector2 original)
    {
        return GetInRange(original, s_verticalHorizontalRange);
    }

    public static IEnumerable<Vector2> GetAllDirections(this Vector2 original)
    {
        return GetInRange(original, new(s_verticalHorizontalRange.x, s_diagonalRange.y));
    }

    private static IEnumerable<Vector2> GetInRange(this Vector2 original, Vector2Int range)
    {
        int[] direction;
        Vector2 current = Vector2.zero;

        for (int i = range.x; i < range.y; i++)
        {
            direction = s_directions[i];
            current.x = original.x * direction[0];
            current.y = original.y * direction[1];

            yield return current;
        }
    }
}