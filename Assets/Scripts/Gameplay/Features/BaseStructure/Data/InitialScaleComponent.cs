using UnityEngine;

public class InitialScaleComponent : IBehaviourComponent
{
    public InitialScaleComponent(Vector3 initialScale)
    {
        InitialScale = initialScale;
    }

    public Vector3 InitialScale { get; }
}