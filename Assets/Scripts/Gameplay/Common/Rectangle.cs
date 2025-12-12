using System;
using UnityEngine;

[Serializable]
public struct Rectangle
{
    [SerializeField] private Vector2 _topLeft;
    [SerializeField] private Vector2 _bottomRight;

    private Vector2? _topRight;
    private Vector2? _bottomLeft;
    private Bounds? _bounds;

    public Rectangle(Vector2 topLeft, Vector2 bottomRight) : this()
    {
        _topLeft = topLeft;
        _bottomRight = bottomRight;
    }

    public Vector2 TopLeft
    {
        get
        {
            if (_topLeft.y < _bottomRight.y)
                _topLeft.y = _bottomRight.y;

            return _topLeft;
        }
    }

    public Vector2 BottomRight
    {
        get
        {
            if (_bottomRight.x < _topLeft.x)
                _bottomRight.x = _topLeft.x;

            return _bottomRight;
        }
    }

    public Vector2 TopRight
    {
        get
        {
            if (_topRight.HasValue == false)
                _topRight = new(BottomRight.x, TopLeft.y);

            return _topRight.Value;
        }
    }

    public Vector2 BottomLeft
    {
        get
        {
            if (_bottomLeft.HasValue == false)
                _bottomLeft = new(TopLeft.x, BottomRight.y);

            return _bottomLeft.Value;
        }
    }

    public float Width => TopRight.x - TopLeft.x;
    public float Height => TopRight.y - BottomRight.y;

    public Bounds Bounds
    {
        get
        {
            if (_bounds.HasValue == false)
                _bounds = GetBounds();

            return _bounds.Value;
        }
    }

    private Bounds GetBounds()
    {
        float ySize = .1f;
        float xCenter = TopLeft.x + Width / 2;
        float zCenter = BottomRight.y + Height / 2;

        Vector3 center = new(xCenter, 0f, zCenter);
        Vector3 size = new(Width, ySize, Height);

        return new(center, size);
    }
}