using System;
using System.Collections;
using UnityEngine;

public class AnimationStateMonitor : IDisposable
{
    private const float CompletitionTime = 1f;

    private readonly Animator _animator;
    private readonly ICoroutineRunner _coroutineRunner;
    private readonly int _targetAnimationHash;
    private readonly Action _onAnimationComplete;

    private readonly WaitUntil _waitAnimationStart;
    private readonly WaitWhile _waitAnimationComplete;
    private IEnumerator _monitorCoroutine;

    private bool _disposed = false;

    public AnimationStateMonitor(Animator animator, string animationName, ICoroutineRunner coroutineRunner, Action onAnimationComplete = null)
    {
        _animator = animator;
        _coroutineRunner = coroutineRunner;
        _targetAnimationHash = Animator.StringToHash(animationName);
        _onAnimationComplete = onAnimationComplete;

        _waitAnimationStart = new(IsStartedAnimation);
        _waitAnimationComplete = new(IsExecutingAnimation);
    }

    public bool IsMonitoring => _monitorCoroutine != null;

    public IEnumerator BeginMonitoring()
    {
        ThrowIf.Invalid(_monitorCoroutine != null, $"{nameof(BeginMonitoring)} should be not beginning yet");

        _monitorCoroutine = MonitorRoutine();

        yield return _monitorCoroutine;
    }

    public void StopMonitoring()
    {
        if (_monitorCoroutine == null)
            return;

        _coroutineRunner?.StopCoroutine(_monitorCoroutine);
        _monitorCoroutine = null;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        StopMonitoring();
        _disposed = true;
    }

    private IEnumerator MonitorRoutine()
    {
        yield return _waitAnimationStart;
        yield return _waitAnimationComplete;

        _onAnimationComplete?.Invoke();
        _monitorCoroutine = null;
    }

    private bool IsStartedAnimation()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).shortNameHash == _targetAnimationHash;
    }

    private bool IsExecutingAnimation()
    {
        return IsStartedAnimation()
            && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < CompletitionTime;
    }
}