using UnityEngine;

public interface IInstantiator
{
    TPrefab Instantiate<TPrefab>(TPrefab original) where TPrefab : UnityEngine.Object;

    TPrefab Instantiate<TPrefab>(TPrefab prefab, Transform parent) where TPrefab : UnityEngine.Object;

    TPrefab Instantiate<TPrefab>(TPrefab original, Transform parent, bool worldPositionStays) where TPrefab : UnityEngine.Object;

    TPrefab Instantiate<TPrefab>(TPrefab original, Vector3 position, Quaternion rotation) where TPrefab : UnityEngine.Object;

    TPrefab Instantiate<TPrefab>(TPrefab original, Vector3 position, Quaternion rotation, Transform parent) where TPrefab : UnityEngine.Object;
}