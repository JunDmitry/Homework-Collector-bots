using System.Collections;

public static class CoroutineExtensions
{
    public static IEnumerator WithCancellation(this IEnumerator enumerator, BehaviourCancellationToken cancellationToken)
    {
        while (enumerator.MoveNext() && cancellationToken?.IsCancellationRequested == false)
        {
            yield return enumerator.Current;
        }
    }
}