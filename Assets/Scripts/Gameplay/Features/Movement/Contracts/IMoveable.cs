using System.Collections;
using UnityEngine;

public interface IMoveable
{
    IEnumerator MoveTo(Vector3 target, float minDistance = .1f);
}