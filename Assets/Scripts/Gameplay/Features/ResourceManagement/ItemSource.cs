using System.Collections;
using UnityEngine;

public class ItemSource<T> : IItemSource<T>
    where T : Component, IDeliverable, IIntractable
{
    public DeliveryOperation<T> PrepareDelivery(T item)
    {
        return new(item.transform.position, item.MinDistanceToTake, ExecutePickup(), r => r.Interact());
    }

    private IEnumerator ExecutePickup()
    {
        yield return null;
    }
}