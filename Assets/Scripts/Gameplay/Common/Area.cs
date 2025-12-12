using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Area
{
    [SerializeField] private Rectangle[] _rectangles;
    [SerializeField] private float _height;

    private RandomPicker<Rectangle> _randomPicker;

    public Area(IEnumerable<Rectangle> rectangles, float height)
    {
        _rectangles = rectangles.ToArray();
        _height = height;
        (XMin, YMin, XMax, YMax) = rectangles.CalculateMinMax();
    }

    public int CountPolygons => _rectangles.Length;

    public IReadOnlyCollection<Rectangle> Rectangles => _rectangles;

    public bool IsEmpty => _rectangles == null || CountPolygons == 0;

    public float Height => _height;
    public float XMin { get; }
    public float YMin { get; }
    public float XMax { get; }
    public float YMax { get; }

    public Vector3 RandomPosition()
    {
        Rectangle randomRectangle = _randomPicker.Pick();
        Bounds bounds = randomRectangle.Bounds;
        float xPosition = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);
        float zPosition = UnityEngine.Random.Range(bounds.min.z, bounds.max.z);

        return new(xPosition, _height, zPosition);
    }

    public void InitializeIfNeed()
    {
        if (_randomPicker != null)
            return;

        _randomPicker = new();

        foreach (Rectangle rectangle in _rectangles)
            _randomPicker.Add(rectangle, rectangle.Width * rectangle.Height);
    }

    public IEnumerator<Rectangle> GetEnumerator()
    {
        foreach (Rectangle rectangle in _rectangles)
            yield return rectangle;
    }

    public void Clear()
    {
        Array.Clear(_rectangles, 0, _rectangles.Length);
        _randomPicker.Clear();
    }
}