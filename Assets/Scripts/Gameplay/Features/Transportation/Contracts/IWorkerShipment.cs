using System;
using System.Collections;

public interface IWorkerShipment<T> : IMoveable
    where T : IDeliverable
{
    bool CanShipment();

    IEnumerator MakeShipment(Action<T> shipmentAction = null);
}