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
    private readonly List<IEventSubscription> _subscriptions;

    private Area _area;
    private Coroutine _spawnCoroutine = null;
    private IEventAggregator _eventAggregator;

    public ResourceSpawner(ResourceSpawnerConfig config, ICoroutineRunner coroutineRunner)
        : this(config, UnityEngine.Random.Range(int.MinValue, int.MaxValue), coroutineRunner)
    { }

    public ResourceSpawner(ResourceSpawnerConfig config, int seed, ICoroutineRunner coroutineRunner)
    {
        _config = config;
        _coroutineStarter = coroutineRunner;
        _area = config.SpawnArea;
        _waitInterval = new(config.SpawnInterval);
        _resourcePicker = new(seed);
        _subscriptions = new();
    }

    public Area SpawnArea => _area;

    public bool Enabled { get; private set; }

    public void Enable()
    {
        if (Enabled)
            return;

        Enabled = true;

        SubscribeAll();

        _spawnCoroutine = _coroutineStarter?.StartCoroutine(Spawn());
        _resourcePicker.Values.ForEach(o => o.Initialize());
    }

    public void Disable()
    {
        if (Enabled == false)
            return;

        Enabled = false;

        _coroutineStarter?.StopCoroutine(_spawnCoroutine);
        _spawnCoroutine = null;

        _subscriptions.ForEach(s => s?.Unsubscribe());
        _subscriptions.Clear();
    }

    public void Initialize(IAssetProvider assetProvider, ISingleParameterFactory<ObjectPool<Resource>, Resource> poolFactory, IEventAggregator eventAggregator)
    {
        Clear();
        _area.InitializeIfNeed();
        _eventAggregator = eventAggregator;

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
        _resourcePicker.Values.ForEach(o => o?.Dispose());
        Disable();
        Clear();
    }

    public void Clear()
    {
        _resourcePicker?.Clear();
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

    private void SubscribeAll()
    {
        _subscriptions.Add(_eventAggregator.Subscribe(new AlwaysTrueCondition<UnitCreatedEvent<BaseStructure>>(), OnStructureCteated));
        _subscriptions.Add(_eventAggregator.Subscribe(new AlwaysTrueCondition<PoolObjectCreatedEvent<Resource>>(), OnResourceCreated));
        _subscriptions.Add(_eventAggregator.Subscribe(new AlwaysTrueCondition<PoolObjectGettedEvent<Resource>>(), OnResourceGetted));
        _subscriptions.Add(_eventAggregator.Subscribe(new AlwaysTrueCondition<PoolObjectReleasedEvent<Resource>>(), OnResourceReleased));
    }

    private void OnStructureCteated(UnitCreatedEvent<BaseStructure> unitCreatedEvent)
    {
        ThrowIf.Null(unitCreatedEvent, nameof(unitCreatedEvent));

        if (_area.IsIntersect(unitCreatedEvent.CreatedUnit.TotalArea) == false)
            return;

        Area newArea = _area.Exclude(unitCreatedEvent.CreatedUnit.TotalArea);

        _area = newArea;
        _area.InitializeIfNeed();
    }

    private void OnResourceCreated(PoolObjectCreatedEvent<Resource> poolResourceCreated)
    {
        Resource resource = poolResourceCreated.PoolObject;

        resource.gameObject.SetActive(false);
        resource.Initialize(_eventAggregator);
    }

    private void OnResourceGetted(PoolObjectGettedEvent<Resource> poolResourceGetted)
    {
        Resource resource = poolResourceGetted.PoolObject;
        ObjectPool<Resource> objectPool = poolResourceGetted.ObjectPool;

        poolResourceGetted.PoolObject.transform.SetPositionAndRotation(_area.RandomPosition(), UnityEngine.Random.rotation);
        resource.gameObject.SetActive(true);
    }

    private void OnResourceReleased(PoolObjectReleasedEvent<Resource> poolResourceReleased)
    {
        Resource resource = poolResourceReleased.PoolObject;
        ObjectPool<Resource> objectPool = poolResourceReleased.ObjectPool;

        resource.gameObject.SetActive(false);
    }
}