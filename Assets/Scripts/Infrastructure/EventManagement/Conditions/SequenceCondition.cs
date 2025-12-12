using System.Collections.Generic;

public class SequenceCondition<TEvent> : ISubscribeCondition<TEvent>
    where TEvent : IEvent
{
    private readonly List<ISubscribeCondition<TEvent>> _conditions;
    private readonly List<ConditionOperationType> _operationTypes;

    public SequenceCondition(ISubscribeCondition<TEvent> startCondition)
    {
        ThrowIf.Null(startCondition, nameof(startCondition));

        _conditions = new()
        {
            startCondition
        };
        _operationTypes = new();
    }

    public int Count => _conditions.Count;

    public static SequenceCondition<TEvent> Create(ISubscribeCondition<TEvent> startCondition)
    {
        return new(startCondition);
    }

    public SequenceCondition<TEvent> Append(ISubscribeCondition<TEvent> condition, ConditionOperationType operationType)
    {
        ThrowIf.Null(condition, nameof(condition));

        _conditions.Add(condition);
        _operationTypes.Add(operationType);

        return this;
    }

    public bool CanSubscribe(IEventSource<TEvent> eventSource)
    {
        ConditionOperationType operationType;
        bool andChainResult = _conditions[0].CanSubscribe(eventSource);

        if (_conditions.Count == 1)
            return andChainResult;

        bool result = false;

        for (int i = 1; i < _conditions.Count; i++)
        {
            operationType = _operationTypes[i - 1];

            if (operationType == ConditionOperationType.And)
            {
                if (andChainResult == false)
                    continue;

                andChainResult = andChainResult && _conditions[i].CanSubscribe(eventSource);
            }
            else
            {
                result = result || andChainResult;

                if (result)
                    return true;

                andChainResult = _conditions[i].CanSubscribe(eventSource);
            }
        }

        return result || andChainResult;
    }
}