using System;
using System.Collections;

public static class DelegateExtensions
{
    public static IEnumerator AsCoroutine(this Action action)
    {
        action?.Invoke();

        yield break;
    }

    public static IEnumerator AsCoroutine<T>(this Action<T> action, T arg)
    {
        action?.Invoke(arg);

        yield break;
    }

    public static IEnumerator AsCoroutine<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
    {
        action?.Invoke(arg1, arg2);

        yield break;
    }
}