using System.Collections;
using UnityEngine;

public class BaseAnimator : MonoBehaviour, ICoroutineRunner
{
    private const string Rotate = nameof(Rotate);
    private const string HideGears = nameof(HideGears);

    private static readonly int s_rotateHash = Animator.StringToHash(Rotate);

    [SerializeField] private Animator _animator;

    private AnimationStateMonitor _endRotateMonitor;

    private void Awake()
    {
        _endRotateMonitor = new(_animator, HideGears, this);
    }

    public void SetRotate()
    {
        _animator.SetBool(s_rotateHash, true);
    }

    public void SetIdle()
    {
        _animator.SetBool(s_rotateHash, false);
    }

    public IEnumerator WaitWhileBuild(float timeInSeconds)
    {
        SetRotate();
        
        yield return new WaitForSeconds(timeInSeconds);

        SetIdle();
        yield return _endRotateMonitor.BeginMonitoring();
    }
}