using System;
using System.Collections;

public interface IWorkerLoader<T> : IMoveable
    where T : IDeliverable
{
    bool CanLoad();

    IEnumerator MakeLoad(T loadable, Action<T> onLoad = null);
}