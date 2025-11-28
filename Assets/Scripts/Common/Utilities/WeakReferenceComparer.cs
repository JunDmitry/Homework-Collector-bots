using System;
using System.Collections.Generic;

public class WeakReferenceComparer<T> : IEqualityComparer<WeakReference<T>>
    where T : class
{
    public bool Equals(WeakReference<T> a, WeakReference<T> b)
    {
        if (a == null && b == null)
            return false;

        if (a == null || b == null)
            return false;

        return a.TryGetTarget(out T aTarget)
            && b.TryGetTarget(out T bTarget)
            && aTarget == bTarget;
    }

    public int GetHashCode(WeakReference<T> obj)
    {
        return obj.TryGetTarget(out T objTarget) && objTarget != null
            ? objTarget.GetHashCode() 
            : 0;
    }
}