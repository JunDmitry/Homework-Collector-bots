using System;
using UnityEngine;

[Serializable]
public struct ScanInfo
{
    [SerializeField] private float _scanRadius;
    [SerializeField] private float _scanInterval;

    public ScanInfo(float scanRadius, float scanInterval)
    {
        _scanRadius = scanRadius;
        _scanInterval = scanInterval;
    }

    public float ScanRadius => _scanRadius;
    public float ScanInterval => _scanInterval;
}