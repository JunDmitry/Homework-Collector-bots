using System;
using System.Collections;

public interface IBaseBehaviour : IDisposable
{
    int Priority { get; }

    bool CanExecute(BehaviourContext behaviourContext = null);

    IEnumerator Execute(BehaviourContext behaviourContext = null, BehaviourCancellationToken cancellationToken = null);
}