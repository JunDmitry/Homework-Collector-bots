using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : IDisposable
{
    private readonly ResourceSpawnerConfig _config;
    private readonly ICoroutineRunner _coroutineStarter;
    private readonly WaitForSeconds _waitInterval;

    private readonly RandomPicker<ObjectPool<Resource>> _resourcePicker;
    private readonly ObservableList<ObjectPool<Resource>> _observableList;
    private readonly Area _area;

    private Coroutine _spawnCoroutine = null;

    public ResourceSpawner(ResourceSpawnerConfig config, ICoroutineRunner coroutineRunner)
        : this(config, UnityEngine.Random.Range(int.MinValue, int.MaxValue), coroutineRunner)
    { }

    public ResourceSpawner(ResourceSpawnerConfig config, int seed, ICoroutineRunner coroutineRunner)
    {
        ResourcePoolSubscriber resourcePoolSubscriber = new(
            r => r.transform.SetPositionAndRotation(_area.RandomPosition(), UnityEngine.Random.rotation));

        _config = config;
        _coroutineStarter = coroutineRunner;
        _area = config.SpawnArea;
        _waitInterval = new(config.SpawnInterval);
        _observableList = new(resourcePoolSubscriber.CreateSubscribeProvider, coroutineRunner);
        _resourcePicker = new(seed);
    }

    public bool Enabled { get; private set; }

    public void Enable()
    {
        if (Enabled)
            return;

        Enabled = true;

        _resourcePicker.Values.ForEach(o =>
        {
            _observableList.Subscribe(o);
            o.Initialize();
        });

        _spawnCoroutine = _coroutineStarter?.StartCoroutine(Spawn());
    }

    public void Disable()
    {
        if (Enabled == false)
            return;

        Enabled = false;
        _observableList?.Unsubscribe();

        _coroutineStarter?.StopCoroutine(_spawnCoroutine);
        _spawnCoroutine = null;
    }

    public void Initialize(IAssetProvider assetProvider, ISingleParameterFactory<ObjectPool<Resource>, Resource> poolFactory)
    {
        Clear();
        _area.InitializeIfNeed();

        Dictionary<ItemDeliverableType, ResourceWeight> weightsMap = _config.GetWeightsAsDictionary();
        assetProvider
            .LoadAllWithWeight<Resource>(
                _config.ResourceAssetPaths,
                r => weightsMap[r.ItemDeliverableType].CalculateWeight(r.Count))
            .ForEach(asset =>
            {
                _resourcePicker.Add(poolFactory.Create(asset.Target), asset.Weight);
            });
    }

    public void Dispose()
    {
        Disable();
        Clear();
    }

    public void Clear()
    {
        _resourcePicker?.Clear();
        _observableList?.Clear();
    }

    private IEnumerator Spawn()
    {
        ObjectPool<Resource> objectPool;

        while (Enabled)
        {
            if (_resourcePicker.Count > 0)
            {
                objectPool = _resourcePicker.Pick();
                objectPool.Get();
            }

            yield return _waitInterval;
        }
    }
}