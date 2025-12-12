using System;

public interface IEventSubscription : IDisposable
{
    bool IsSubscribed { get; }

    void Unsubscribe();
}