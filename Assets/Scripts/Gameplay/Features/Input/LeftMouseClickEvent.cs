using UnityEngine;

public class LeftMouseClickEvent : IEvent
{
    public LeftMouseClickEvent(Vector3 screenPoint)
    {
        ScreenPoint = screenPoint;
    }

    public Vector3 ScreenPoint { get; }
}