using System;

public class SourcePropertyCondition<TEvent, TProperty> : ISubscribeCondition<TEvent>
    where TEvent : IEvent
{
    private readonly Func<IEventSource<TEvent>, TProperty> _propertySelector;
    private readonly Func<TProperty, bool> _condition;

    public SourcePropertyCondition(
        Func<IEventSource<TEvent>, TProperty> propertySelector,
        Func<TProperty, bool> condition)
    {
        ThrowIf.Null(propertySelector, nameof(propertySelector));
        ThrowIf.Null(condition, nameof(condition));

        _propertySelector = propertySelector;
        _condition = condition;
    }

    public bool CanSubscribe(IEventSource<TEvent> eventSource)
    {
        return _condition(_propertySelector(eventSource));
    }
}