using System.Collections;
using UnityEngine;

public class WorkerAnimator : MonoBehaviour, ICoroutineRunner
{
    private const string Grab = nameof(Grab);
    private const string Bring = nameof(Bring);

    private static readonly int s_movingHash = Animator.StringToHash("Moving");
    private static readonly int s_grabHash = Animator.StringToHash(Grab);
    private static readonly int s_bringHash = Animator.StringToHash(Bring);

    [SerializeField] private Animator _animator;

    private AnimationStateMonitor _grabMonitor;
    private AnimationStateMonitor _bringMonitor;

    private void Awake()
    {
        _grabMonitor = new(_animator, Grab, this, SetIdle);
        _bringMonitor = new(_animator, Bring, this, SetIdle);
    }

    public void SetIdle()
    {
        _animator.SetBool(s_movingHash, false);
    }

    public void SetMoving()
    {
        _animator.SetBool(s_movingHash, true);
    }

    public void SetGrab()
    {
        _animator.SetTrigger(s_grabHash);
    }

    public void SetBring()
    {
        _animator.SetTrigger(s_bringHash);
    }

    public IEnumerator WaitWhileGrabbing()
    {
        if (_grabMonitor.IsMonitoring)
            yield break;

        yield return _grabMonitor.BeginMonitoring();
    }

    public IEnumerator WaitWhileBringing()
    {
        if (_bringMonitor.IsMonitoring)
            yield break;

        yield return _bringMonitor.BeginMonitoring();
    }
}