using System;
using UnityEngine;

[Serializable]
public struct ResourceWeight
{
    [SerializeField] private float _singleCountWeight;
    [SerializeField] private float _weightDivisionMultiplierPerCount;

    public readonly float SingleCountWeight => _singleCountWeight;
    public readonly float WeightDivisionMultiplierPerCount => _weightDivisionMultiplierPerCount;

    public float CalculateWeight(int resourceCount)
    {
        int startDivision = 1;
        int maxIgnoreDivisionPerCount = 1;

        return SingleCountWeight / (startDivision + (resourceCount - maxIgnoreDivisionPerCount) * WeightDivisionMultiplierPerCount);
    }
}