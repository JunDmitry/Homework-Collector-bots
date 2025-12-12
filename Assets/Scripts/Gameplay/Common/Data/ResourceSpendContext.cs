using System;
using UnityEngine;

[Serializable]
public struct ResourceSpendContext
{
    [SerializeField] private int _stoneCount;
    [SerializeField] private int _woodCount;
    [SerializeField] private int _ironCount;

    public ResourceSpendContext(int stoneCount = 0, int woodCount = 0, int ironCount = 0)
    {
        _stoneCount = stoneCount;
        _woodCount = woodCount;
        _ironCount = ironCount;
    }

    public int StoneCount => _stoneCount;
    public int WoodCount => _woodCount;
    public int IronCount => _ironCount;
}