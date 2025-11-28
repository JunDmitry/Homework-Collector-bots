using System;
using System.Collections.Generic;
using UnityEngine;

public class Scanner : IScanner
{
    private Collider[] _collidersBuffer;
    private float _radius;

    public Scanner(float radius, int bufferSize = 128)
    {
        if (bufferSize <= 0)
            throw new ArgumentException($"{nameof(bufferSize)} must be positive", nameof(bufferSize));

        _radius = radius;
        _collidersBuffer = new Collider[bufferSize];
    }

    public IEnumerable<TComponent> Scan<TComponent>(Vector3 scanCenter, int layer = Physics.AllLayers)
        where TComponent : Component
    {
        int hitsCount = Physics.OverlapSphereNonAlloc(scanCenter, _radius, _collidersBuffer, layer);

        for (int i = 0; i < hitsCount; i++)
        {
            Collider hit = _collidersBuffer[i];

            if (hit.TryGetComponent(out TComponent component))
                yield return component;
        }
    }
}