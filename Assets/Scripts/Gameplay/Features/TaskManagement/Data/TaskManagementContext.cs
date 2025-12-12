using System;
using UnityEngine;

public readonly struct TaskManagementContext<T>
    where T : Component, IDeliverable, IIntractable
{
    public TaskManagementContext(
        ICoroutineRunner coroutineRunner,
        IEventAggregator eventAggregator,
        OwnerInfo ownerInfo,
        TaskQueue<T> taskQueue,
        TransportationTaskAssigner<T> taskAssigner,
        TaskManagementInfo taskManagementInfo,
        Action<UpdatedCountInfoEvent> updatedCountInfo,
        float assignInterval,
        float synchronizeInterval = 1f)
    {
        ThrowIf.Null(coroutineRunner, nameof(coroutineRunner));
        ThrowIf.Null(eventAggregator, nameof(eventAggregator));
        ThrowIf.Null(ownerInfo, nameof(ownerInfo));
        ThrowIf.Null(taskQueue, nameof(taskQueue));
        ThrowIf.Null(taskAssigner, nameof(taskAssigner));
        ThrowIf.Null(taskManagementInfo, nameof(taskManagementInfo));
        ThrowIf.Invalid(assignInterval <= 0, $"{nameof(assignInterval)} should be positive");
        ThrowIf.Invalid(synchronizeInterval <= 0, $"{nameof(synchronizeInterval)} should be positive");

        CoroutineRunner = coroutineRunner;
        EventAggregator = eventAggregator;
        OwnerInfo = ownerInfo;
        TaskQueue = taskQueue;
        TaskAssigner = taskAssigner;
        TaskManagementInfo = taskManagementInfo;
        UpdatedCountInfo = updatedCountInfo;
        AssignInterval = assignInterval;
        SynchronizeInterval = synchronizeInterval;
    }

    public ICoroutineRunner CoroutineRunner { get; }
    public IEventAggregator EventAggregator { get; }
    public OwnerInfo OwnerInfo { get; }
    public TaskQueue<T> TaskQueue { get; }
    public TransportationTaskAssigner<T> TaskAssigner { get; }
    public TaskManagementInfo TaskManagementInfo { get; }
    public Action<UpdatedCountInfoEvent> UpdatedCountInfo { get; }
    public float AssignInterval { get; }
    public float SynchronizeInterval { get; }
}