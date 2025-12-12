using UnityEngine;

public class AreaRaycaster
{
    private readonly float _maxRaycastDistance;

    public AreaRaycaster(float maxRaycastDistance)
    {
        _maxRaycastDistance = maxRaycastDistance;
    }

    public bool CanPlaceBuilding(Vector3 rayPoint, Area area)
    {
        float height = .1f;
        float multiplier = .5f;
        float width = area.XMax - area.XMin;
        float length = area.YMax - area.YMin;
        Vector2 offset = new(width * multiplier, length * multiplier);

        bool canPlace = Physics.CheckBox(
            new(rayPoint.x, rayPoint.y + area.Height + height, rayPoint.z),
            new(offset.x, height, offset.y)) == false;
        bool hasGround = true;

        foreach (Vector2 direction in offset.GetDiagonals())
        {
            bool raycastResult = Physics.Raycast(
               new Vector3(rayPoint.x + direction.x, rayPoint.y + area.Height, rayPoint.z + direction.y),
               Vector3.down,
               _maxRaycastDistance + area.Height);
            hasGround &= raycastResult;
        }

        return canPlace && hasGround;
    }
}