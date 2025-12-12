using System;

public interface IEventSource<out TEvent>
    where TEvent : IEvent
{
    event Action<TEvent> EventRaised;

    bool IsActive { get; }
}