using System;
using System.Collections;
using UnityEngine;

public readonly struct DeliveryOperation<T>
{
    public DeliveryOperation(Vector3 position, float minDistance, IEnumerator executionCoroutine, Action<T> onEndOperation = null)
    {
        Position = position;
        MinDistance = minDistance;
        ExecutionCoroutine = executionCoroutine;
        OnEndOperation = onEndOperation;
    }

    public Vector3 Position { get; }
    public float MinDistance { get; }
    public IEnumerator ExecutionCoroutine { get; }
    public Action<T> OnEndOperation { get; }
}