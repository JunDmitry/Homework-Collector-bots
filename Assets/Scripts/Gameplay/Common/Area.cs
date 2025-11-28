using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Area
{
    [SerializeField] private Rectangle[] _rectangles;
    [SerializeField] private float _height;

    private RandomPicker<Rectangle> _randomPicker;

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
}