using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WorkerMover
{
    private readonly NavMeshAgent _agent;
    private readonly WorkerAnimator _animator;
    private readonly float _stopDistance;

    private bool _isMoving;

    public WorkerMover(NavMeshAgent agent, WorkerAnimator animator, float stopDistance = .1f)
    {
        _agent = agent;
        _animator = animator;
        _stopDistance = stopDistance;
        _agent.autoBraking = true;
    }

    public event Action StartedMovement;

    public event Action StoppedMovement;

    public event Action CompletedMovement;

    public bool IsMoving => _isMoving;
    public bool IsSuccessfulMove { get; private set; }

    public IEnumerator MoveTo(Vector3 target, float? overrideStopDistance = null)
    {
        if (_isMoving)
            Stop();

        _isMoving = true;
        IsSuccessfulMove = false;

        _animator.SetMoving();
        StartedMovement?.Invoke();
        _agent.isStopped = false;
        _agent.SetDestination(target);

        yield return BeginMovementCompleteRoutine(overrideStopDistance ?? _stopDistance);
    }

    public void Stop()
    {
        if (_isMoving == false)
            return;

        CompleteMovement();
        IsSuccessfulMove = false;
        StoppedMovement?.Invoke();
    }

    private IEnumerator BeginMovementCompleteRoutine(float stopDistance)
    {
        yield return new WaitUntil(() => _agent.hasPath);

        while (_agent.hasPath
            && _agent.remainingDistance > stopDistance
            && _agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            yield return null;
        }

        if (_isMoving && _agent.remainingDistance <= stopDistance)
        {
            CompleteMovement();
            IsSuccessfulMove = true;
            CompletedMovement?.Invoke();
        }
        else
        {
            Stop();
        }
    }

    private void CompleteMovement()
    {
        _isMoving = false;
        _agent.isStopped = true;
        _agent.ResetPath();
        _animator.SetIdle();
    }
}