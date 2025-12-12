using System;
using System.Collections;
using System.Collections.Generic;

public class BehaviourRunner : IDisposable
{
    private readonly Dictionary<IBaseBehaviour, BehaviourCancellationTokenSource> _tokenSourceByBehaviour;
    private readonly ICoroutineRunner _coroutineRunner;

    public BehaviourRunner(ICoroutineRunner coroutineRunner)
    {
        _tokenSourceByBehaviour = new();
        _coroutineRunner = coroutineRunner;
    }

    public void Dispose()
    {
        _tokenSourceByBehaviour.ForEach(p => p.Value.Dispose());
        _tokenSourceByBehaviour.Clear();
    }

    public void ExecuteBehaviour(
        IBaseBehaviour baseBehaviour,
        BehaviourContext behaviourContext = null,
        BehaviourCancellationToken parentToken = null)
    {
        ThrowIf.Null(baseBehaviour, nameof(baseBehaviour));
        ThrowIf.Invalid(IsExecute(baseBehaviour), $"{nameof(ExecuteBehaviour)} already execute");

        behaviourContext ??= new();
        BehaviourCancellationTokenSource tokenSource = new();

        parentToken?.RegisterCallback(() => tokenSource.Cancel());
        _tokenSourceByBehaviour[baseBehaviour] = tokenSource;
        _coroutineRunner.StartCoroutine(ExecuteBehaviourRoutine(baseBehaviour, behaviourContext, tokenSource.Token));
    }

    public void CancelBehaviour(IBaseBehaviour baseBehaviour)
    {
        if (_tokenSourceByBehaviour.TryGetValue(baseBehaviour, out BehaviourCancellationTokenSource source))
            source.Cancel();
    }

    public bool IsExecute(IBaseBehaviour baseBehaviour)
    {
        return _tokenSourceByBehaviour.ContainsKey(baseBehaviour);
    }

    public bool CanInterrupt(IBaseBehaviour baseBehaviour)
    {
        if (_tokenSourceByBehaviour.TryGetValue(baseBehaviour, out BehaviourCancellationTokenSource source) == false)
            return false;

        return source.Token.CanInterrupt;
    }

    private IEnumerator ExecuteBehaviourRoutine(IBaseBehaviour baseBehaviour, BehaviourContext behaviourContext, BehaviourCancellationToken token)
    {
        try
        {
            if (baseBehaviour.CanExecute(behaviourContext) == false)
                yield break;

            yield return baseBehaviour.Execute(behaviourContext, token);
        }
        finally
        {
            _tokenSourceByBehaviour.Remove(baseBehaviour);
        }
    }
}