public abstract class NamedSubscribeCondition<TEvent> : ISubscribeCondition<TEvent>
    where TEvent : IEvent
{
    public NamedSubscribeCondition(string conditionName)
    {
        ThrowIf.Null(conditionName, nameof(conditionName));

        ConditionName = conditionName;
    }

    public string ConditionName { get; }

    public abstract bool CanSubscribe(IEventSource<TEvent> eventSource);
}