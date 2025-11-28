using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataLoader<TStaticType> : DataLoader
    where TStaticType : ScriptableObject
{
    private readonly string _directoryPath;

    private List<TStaticType> _prefabs;

    public DataLoader(string directoryPath)
    {
        _directoryPath = directoryPath;
    }

    public IReadOnlyList<TStaticType> Prefabs => _prefabs.AsReadOnly();

    public override void Load()
    {
        _prefabs = Resources
            .LoadAll<TStaticType>(_directoryPath)
            .ToList();
    }
}