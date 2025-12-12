using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseStructure : Structure, ICoroutineRunner
{
    [SerializeField] private BaseStructureConfig _config;
    [SerializeField] private BuilderConfig _builderConfig;
    [SerializeField] private BaseAnimator _animator;
    [SerializeField] private Flag _flagPrefab;

    private BaseStructureAreaFacade _areaFacade;

    private TaskManagement<Resource> _taskManagement;
    private UniqueItemCollection<Resource> _uniqueItemCollection;
    private ITwoParametersFactory<ITransportationWorker<Resource>, TransportationWorker, Vector3> _workerFactory;
    private IEventAggregator _eventAggregator;
    private Storage<Resource> _resourceStorage;
    private FlagPlacer _flagPlacer;

    private BuilderManagement _builderManagement;
    private Coroutine _builderRoutine;

    public Vector3 WaitArea => _areaFacade.WaitArea;
    public Vector3 ShipmentArea => _areaFacade.ShipmentArea;

    public Area TotalArea
    {
        get
        {
            if (_areaFacade == null)
            {
                _areaFacade = new(_config, () => transform.position);
                _areaFacade.InitializeIfNeed();
            }

            return _areaFacade.TotalArea;
        }
    }

    public ScanInfo ScanInfo => _config.ScanInfo;
    public bool EnabledLogics { get; private set; }

    private void OnDestroy()
    {
        _uniqueItemCollection?.Dispose();
        _taskManagement?.Dispose();
        _builderManagement?.Dispose();
    }

    public void EnableLogics()
    {
        if (EnabledLogics)
            return;

        EnabledLogics = true;
        _uniqueItemCollection.Enable();
        _taskManagement.Enable();
        _builderRoutine = StartCoroutine(BeginBehaviour());
    }

    public void DisableLogics()
    {
        if (EnabledLogics == false)
            return;

        _uniqueItemCollection.Disable();
        _taskManagement.Disable();
        StopCoroutine(_builderRoutine);
    }

    public void Initialize(
        IntervalScanner<Resource> scanner,
        Storage<Resource> storage,
        ITwoParametersFactory<ITransportationWorker<Resource>, TransportationWorker, Vector3> workerFactory,
        TaskManagement<Resource> taskManagement,
        BuilderManagement builderManagement,
        IEventAggregator eventAggregator)
    {
        OwnerInfo ownerInfo = new(GetInstanceID(), transform.position);
        _eventAggregator = eventAggregator;
        _resourceStorage = storage;
        _taskManagement = taskManagement;
        _uniqueItemCollection = new(scanner, eventAggregator);
        _workerFactory = workerFactory;
        _builderManagement = builderManagement;

        _flagPlacer = new(
            ownerInfo,
            Instantiate(_flagPrefab, transform.position, Quaternion.identity, transform),
            _eventAggregator);

        _resourceStorage.Initialize(() => ShipmentArea, _config.MinShipmentDistance);
        _taskManagement.Initialize(
            () => WaitArea,
            _config.TaskAssignInterval,
            CreateSubscriptionsForTaskManagement(),
            ownerInfo);
        _taskManagement.Enable();

        BaseStructureInitializeContext initializeContext = new(
            this, _config,
            _workerFactory, _builderConfig,
            _resourceStorage, _animator,
            _eventAggregator, _flagPlacer,
            _taskManagement, ownerInfo);

        InitializeBehaviours(initializeContext);
    }

    private IEnumerator BeginBehaviour()
    {
        while (EnabledLogics)
        {
            yield return _builderManagement.Execute();
        }
    }

    public void PlaceFlag(Vector3 position)
    {
        _flagPlacer.TryPlace(position);
    }

    private IEnumerable<ISubscribeProvider> CreateSubscriptionsForTaskManagement()
    {
        ISubscribeProvider uniqueSubscription = SubscribeProvider<UniqueItemCollection<Resource>, Action<Resource>>.Create(
            _uniqueItemCollection,
            _taskManagement.OnFoundedUniqueItem,
            (s, h) => s.FoundedUniqueItem += h,
            (s, h) => s.FoundedUniqueItem -= h);

        yield return uniqueSubscription;
    }

    private void InitializeBehaviours(BaseStructureInitializeContext initializeContext)
    {
        BaseBehaviourInitializer behaviourInitializer = new(initializeContext);
        BehaviourAddContext<ITransportationWorker<Resource>> behaviourAddContext;

        if (_config.InitialWorkerCount > 0)
        {
            behaviourAddContext = behaviourInitializer.CreateInitialWorkerContext();

            _builderManagement.AddCountUseBehaviour(
                behaviourAddContext.BuildBehaviourContext,
                behaviourAddContext.TimeBehaviourContext,
                _config.InitialWorkerCount,
                behaviourAddContext.AdditionalBehaviours);
        }

        behaviourAddContext = behaviourInitializer.CreateConstantWorkerContext();
        BehaviourAddContext<BaseStructure> colonizationAddContext = behaviourInitializer.CreateColonizationContext();

        _builderManagement.AddBehaviour(
            behaviourAddContext.BuildBehaviourContext,
            behaviourAddContext.TimeBehaviourContext,
            behaviourAddContext.AdditionalBehaviours);
        _builderManagement.AddBehaviour(
            colonizationAddContext.BuildBehaviourContext,
            colonizationAddContext.TimeBehaviourContext,
            colonizationAddContext.AdditionalBehaviours);
    }

    private void OnDrawGizmos()
    {
        Vector3 offset = new(transform.position.x, 0f, transform.position.z);

        Gizmos.color = new Color(1, 1, 0, .5f);
        DrawArea(offset, _config.WaitArea);

        Gizmos.color = new Color(0, 1, 0, .5f);
        DrawArea(offset, _config.ShipmentArea);
    }

    private void DrawArea(Vector3 offset, Area area)
    {
        foreach (Rectangle rectangle in area)
            Gizmos.DrawCube(rectangle.Bounds.center + offset, rectangle.Bounds.size);
    }
}