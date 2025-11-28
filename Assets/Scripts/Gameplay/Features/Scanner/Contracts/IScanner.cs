using System.Collections.Generic;
using UnityEngine;

public interface IScanner
{
    IEnumerable<TComponent> Scan<TComponent>(Vector3 scanCenter, int layer = Physics.AllLayers)
        where TComponent : Component;
}