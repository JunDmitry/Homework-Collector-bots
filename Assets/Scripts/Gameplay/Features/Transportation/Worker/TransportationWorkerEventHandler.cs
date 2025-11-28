using System;
using UnityEngine;

public class TransportationWorkerEventHandler : MonoBehaviour
{
    public event Action Grabbing;

    public event Action Bringing;

    public void InvokeGrabbingEvent()
    {
        Grabbing?.Invoke();
    }

    public void InvokeBringingEvent()
    {
        Bringing?.Invoke();
    }
}