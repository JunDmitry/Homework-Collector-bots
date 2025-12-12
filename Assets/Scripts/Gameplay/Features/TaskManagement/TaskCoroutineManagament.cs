using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskCoroutineManagament<T> : IDisposable
    where T : Component, IDeliverable, IIntractable
{
    private readonly List<Coroutine> _coroutines;
    private readonly TaskQueue<T> _taskQueue;
    private readonly TransportationTaskAssigner<T> _taskAssigner;
    private readonly TaskManagementInfo _taskManagementInfo;
    private readonly Action<UpdatedCountInfoEvent> _updatedCountInfo;
    private readonly ICoroutineRunner _coroutineRunner;

    private readonly WaitForSeconds _waitAssign;
    private readonly WaitForSeconds _waitSynchronize;

    public TaskCoroutineManagament(TaskManagementContext<T> taskManagementContext)
    {
        _coroutines = new();
        _taskQueue = taskManagementContext.TaskQueue;
        _taskAssigner = taskManagementContext.TaskAssigner;
        _taskManagementInfo = taskManagementContext.TaskManagementInfo;
        _updatedCountInfo = taskManagementContext.UpdatedCountInfo;
        _coroutineRunner = taskManagementContext.CoroutineRunner;
        _waitAssign = new WaitForSeconds(taskManagementContext.AssignInterval);
        _waitSynchronize = new WaitForSeconds(taskManagementContext.SynchronizeInterval);
    }

    public bool Enabled { get; private set; }

    public void Dispose()
    {
        Disable();
    }

    public void Enable()
    {
        if (Enabled)
            return;

        Enabled = true;
        _coroutines.Add(_coroutineRunner.StartCoroutine(AssignTaskRoutine()));
        _coroutines.Add(_coroutineRunner.StartCoroutine(SynchronizeCountInfoRoutine()));
    }

    public void Disable()
    {
        if (Enabled == false)
            return;

        Enabled = false;
        _coroutines.ForEach(c => _coroutineRunner?.StopCoroutine(c));
        _coroutines.Clear();
    }

    private IEnumerator AssignTaskRoutine()
    {
        TransportationTask<T> task;

        while (Enabled)
        {
            while (_taskQueue.ReadyToAssignCount > 0 && _taskAssigner.CanAssignTask())
            {
                task = _taskQueue.Dequeue();
                _taskAssigner.AssignTask(task);

                _taskManagementInfo.MoveToAssigned();
            }

            yield return _waitAssign;
        }
    }

    private IEnumerator SynchronizeCountInfoRoutine()
    {
        UpdatedCountInfoEvent updatedCountInfo = new(_taskManagementInfo);

        while (Enabled)
        {
            _taskManagementInfo.SynchronizeReadyAssignValue(_taskQueue.ReadyToAssignCount);
            _updatedCountInfo?.Invoke(updatedCountInfo);

            yield return _waitSynchronize;
        }
    }
}