using System;
using System.Collections.Generic;
using UnityEngine;

public class StaticDataProvider : IStaticDataProvider
{
    private const string ResourceSpawnerConfigsPath = "Configs/";

    private readonly Dictionary<Type, DataLoader> _dataLoaderByType;

    private bool _initialized;

    public StaticDataProvider()
    {
        _dataLoaderByType = new();

        CreateAndAddConfig<ResourceSpawnerConfig>(ResourceSpawnerConfigsPath);
    }

    public void LoadAll()
    {
        if (_initialized)
            return;

        _initialized = true;

        _dataLoaderByType.Keys
            .ForEach(loader => _dataLoaderByType[loader].Load());
    }

    public bool TryGetConfigs<TStaticType>(out IReadOnlyList<TStaticType> configs)
        where TStaticType : ScriptableObject
    {
        configs = default;

        if (_dataLoaderByType.TryGetValue(typeof(TStaticType), out DataLoader dataLoader) == false)
            return false;

        if (dataLoader is not DataLoader<TStaticType> genericLoader)
            return false;

        configs = genericLoader.Prefabs;

        return true;
    }

    private void CreateAndAddConfig<TStaticType>(string directoryPath)
        where TStaticType : ScriptableObject
    {
        DataLoader<TStaticType> dataLoader = new(directoryPath);

        _dataLoaderByType[typeof(TStaticType)] = dataLoader;
    }
}