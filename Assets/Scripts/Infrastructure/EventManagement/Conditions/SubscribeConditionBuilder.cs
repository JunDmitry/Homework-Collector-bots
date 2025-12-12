using System;
using System.Collections.Generic;

public class SubscribeConditionBuilder<TEvent>
    where TEvent : IEvent
{
    private readonly Stack<SequenceCondition<TEvent>> _sequenceConditions;

    public SubscribeConditionBuilder(ISubscribeCondition<TEvent> first)
    {
        ThrowIf.Null(first, nameof(first));

        _sequenceConditions = new();
        _sequenceConditions.Push(new SequenceCondition<TEvent>(first));
    }

    public SubscribeConditionBuilder<TEvent> AndSourceType<TAsType>()
        where TAsType : class, IEventSource<TEvent>
    {
        return AppendSourceType<TAsType>(ConditionOperationType.And);
    }

    public SubscribeConditionBuilder<TEvent> OrSourceType<TAsType>()
        where TAsType : class, IEventSource<TEvent>
    {
        return AppendSourceType<TAsType>(ConditionOperationType.Or);
    }

    public SubscribeConditionBuilder<TEvent> AndSourceProperty<TProperty>(
        Func<IEventSource<TEvent>, TProperty> propertySelector,
        Func<TProperty, bool> condition)
    {
        return AppendSourceProperty(propertySelector, condition, ConditionOperationType.And);
    }

    public SubscribeConditionBuilder<TEvent> OrSourceProperty<TProperty>(
        Func<IEventSource<TEvent>, TProperty> propertySelector,
        Func<TProperty, bool> condition)
    {
        return AppendSourceProperty(propertySelector, condition, ConditionOperationType.Or);
    }

    public SubscribeConditionBuilder<TEvent> AndSequence(ISubscribeCondition<TEvent> firstCondition)
    {
        ThrowIf.Null(firstCondition, nameof(firstCondition));

        SequenceCondition<TEvent> sequenceCondition = new(firstCondition);

        AppendAnd(sequenceCondition);
        _sequenceConditions.Push(sequenceCondition);

        return this;
    }

    public SubscribeConditionBuilder<TEvent> OrSequence(ISubscribeCondition<TEvent> firstCondition)
    {
        ThrowIf.Null(firstCondition, nameof(firstCondition));

        SequenceCondition<TEvent> sequenceCondition = new(firstCondition);

        AppendOr(sequenceCondition);
        _sequenceConditions.Push(sequenceCondition);

        return this;
    }

    public SubscribeConditionBuilder<TEvent> EndSequence()
    {
        ThrowIf.Invalid(_sequenceConditions.Count <= 1, $"{nameof(EndSequence)} can't be executed when sequence count less than 2");

        _sequenceConditions.Pop();

        return this;
    }

    public ISubscribeCondition<TEvent> Build()
    {
        ISubscribeCondition<TEvent> result = _sequenceConditions.Pop();

        while (_sequenceConditions.Count > 0)
            result = _sequenceConditions.Pop();

        return result;
    }

    private SubscribeConditionBuilder<TEvent> AppendSourceType<TAsType>(
    ConditionOperationType operationType)
    where TAsType : class, IEventSource<TEvent>
    {
        SourceTypeCondition<TEvent, TAsType> condition = new();
        AppendCondition(condition, operationType);

        return this;
    }

    private SubscribeConditionBuilder<TEvent> AppendSourceProperty<TProperty>(
        Func<IEventSource<TEvent>, TProperty> propertySelector,
        Func<TProperty, bool> condition,
        ConditionOperationType operationType)
    {
        ThrowIf.Null(propertySelector, nameof(propertySelector));
        ThrowIf.Null(condition, nameof(condition));

        SourcePropertyCondition<TEvent, TProperty> propertyCondition = new(propertySelector, condition);
        AppendCondition(propertyCondition, operationType);

        return this;
    }

    private void AppendCondition(
        ISubscribeCondition<TEvent> condition,
        ConditionOperationType operationType)
    {
        if (operationType == ConditionOperationType.And)
            AppendAnd(condition);
        else
            AppendOr(condition);
    }

    private void AppendAnd(ISubscribeCondition<TEvent> subscribeCondition)
    {
        _sequenceConditions.Peek().Append(subscribeCondition, ConditionOperationType.And);
    }

    private void AppendOr(ISubscribeCondition<TEvent> subscribeCondition)
    {
        _sequenceConditions.Peek().Append(subscribeCondition, ConditionOperationType.Or);
    }
}