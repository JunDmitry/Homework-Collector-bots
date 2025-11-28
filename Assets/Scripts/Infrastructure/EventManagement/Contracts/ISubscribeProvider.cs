using System;

public interface ISubscribeProvider : IDisposable
{
    bool IsSubscribed { get; }

    void Subscribe();

    void Unsubscribe();
}