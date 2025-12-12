using System;
using UnityEngine;

[Serializable]
public struct StructureBuildInfo
{
    [SerializeField] private ResourceSpendContext _spendContext;
    [SerializeField] private float _buildTimeInSeconds;

    public ResourceSpendContext SpendContext => _spendContext;
    public float BuildTimeInSeconds => _buildTimeInSeconds;
}