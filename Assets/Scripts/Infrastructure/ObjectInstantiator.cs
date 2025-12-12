using UnityEngine;

public class ObjectInstantiator : IInstantiator
{
    public TPrefab Instantiate<TPrefab>(TPrefab original) where TPrefab : Object
    {
        return Object.Instantiate(original);
    }

    public TPrefab Instantiate<TPrefab>(TPrefab prefab, Transform parent) where TPrefab : Object
    {
        return Object.Instantiate(prefab, parent);
    }

    public TPrefab Instantiate<TPrefab>(TPrefab original, Transform parent, bool worldPositionStays) where TPrefab : Object
    {
        return Object.Instantiate(original, parent, worldPositionStays);
    }

    public TPrefab Instantiate<TPrefab>(TPrefab original, Vector3 position, Quaternion rotation) where TPrefab : Object
    {
        return Object.Instantiate(original, position, rotation);
    }

    public TPrefab Instantiate<TPrefab>(TPrefab original, Vector3 position, Quaternion rotation, Transform parent) where TPrefab : Object
    {
        return Object.Instantiate(original, position, rotation, parent);
    }
}