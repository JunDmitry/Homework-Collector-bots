using UnityEngine;

public readonly struct OwnerInfo
{
    public OwnerInfo(int instanceId, Vector3 position)
    {
        InstanceId = instanceId;
        Position = position;
    }

    public int InstanceId { get; }
    public Vector3 Position { get; }
}