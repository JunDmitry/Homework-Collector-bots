using UnityEngine;

public class GroundClickedEvent : IEvent
{
    public GroundClickedEvent(Vector3 clickPosition)
    {
        ClickPosition = clickPosition;
    }

    public Vector3 ClickPosition { get; }
}