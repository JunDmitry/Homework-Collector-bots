using System;
using UnityEngine;

public class BaseStructureAreaFacade
{
    private readonly BaseStructureConfig _structureConfig;
    private readonly Func<Vector3> _getPosition;
    
    private Area _totalArea;

    public BaseStructureAreaFacade(BaseStructureConfig structureConfig, Func<Vector3> getPosition)
    {
        ThrowIf.Null(structureConfig, nameof(structureConfig));
        ThrowIf.Null(getPosition, nameof(getPosition));

        _structureConfig = structureConfig;
        _getPosition = getPosition;
    }

    public Vector3 WaitArea => _structureConfig.WaitArea.RandomPosition() + _getPosition();
    public Vector3 ShipmentArea => _structureConfig.ShipmentArea.RandomPosition() + _getPosition();
    public Area TotalArea
    {
        get
        {
            _totalArea ??= CalculateTotalArea();

            return _totalArea;
        }
    }

    public void InitializeIfNeed()
    {
        _structureConfig.WaitArea.InitializeIfNeed();
        _structureConfig.ShipmentArea.InitializeIfNeed();
    }

    private Area CalculateTotalArea()
    {
        Vector3 position = _getPosition();
        (float xMin, float yMin, float xMax, float yMax) = _structureConfig.WaitArea.CalculateMinMax();
        Vector2 topLeft = new(xMin, yMax);
        Vector2 bottomRight = new(xMax, yMin);
        (xMin, yMin, xMax, yMax) = _structureConfig.ShipmentArea.CalculateMinMax();

        topLeft.x = Math.Min(topLeft.x, xMin);
        topLeft.y = Math.Max(topLeft.y, yMax);
        bottomRight.x = Math.Max(bottomRight.x, xMax);
        bottomRight.y = Math.Min(bottomRight.y, yMin);

        Vector2 offset = new(position.x, position.z);

        return new(new Rectangle[] { new(topLeft + offset, bottomRight + offset) }, _structureConfig.WaitArea.Height);
    }
}