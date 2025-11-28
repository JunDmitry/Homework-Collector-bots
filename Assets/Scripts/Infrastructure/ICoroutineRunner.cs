using System.Collections;
using UnityEngine;

public interface ICoroutineRunner
{
    Coroutine StartCoroutine(IEnumerator routine);

    void StopCoroutine(IEnumerator coroutine);

    void StopCoroutine(Coroutine coroutine);
}
