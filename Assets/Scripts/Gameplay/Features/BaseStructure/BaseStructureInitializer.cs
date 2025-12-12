using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseStructureInitializer
{
    private static readonly IEnumerable<ITransportationWorker<Resource>> s_emptyWorkers = Array.Empty<ITransportationWorker<Resource>>();

    private readonly IEventAggregator _eventAggregator;
    private readonly ITwoParametersFactory<ITransportationWorker<Resource>, TransportationWorker, Vector3> _workerFactory;
    private readonly ICoroutineRunner _coroutineRunner;

    private SingleParameterFactory<TransportationTask<Resource>, DeliveryContext<Resource>> _taskFactory;
    private ITwoParametersFactory<SpendBehaviour, ResourceSpendRequest, int> _spendBehaviourFactory;
    private ITwoParametersFactory<TimeBehaviour, TimeBehaviourContext, int> _timeBehaviourFactory;

    public BaseStructureInitializer(
        IEventAggregator aggregator,
        ICoroutineRunner coroutineRunner,
        ITwoParametersFactory<ITransportationWorker<Resource>, TransportationWorker, Vector3> workerFactory)
    {
        ThrowIf.Null(aggregator, nameof(aggregator));
        ThrowIf.Null(workerFactory, nameof(workerFactory));

        _eventAggregator = aggregator;
        _coroutineRunner = coroutineRunner;
        _workerFactory = workerFactory;

        InitializeFactories();
    }

    public void Initialize(BaseStructure structure, IEnumerable<ITransportationWorker<Resource>> initialWorkers = null)
    {
        initialWorkers ??= s_emptyWorkers;
        OwnerInfo ownerInfo = new(structure.GetInstanceID(), structure.transform.position);

        IScanner scanner = new Scanner(structure.ScanInfo.ScanRadius);
        IntervalScanner<Resource> intervalScanner = new(
            scanner, structure.ScanInfo.ScanInterval,
            () => structure.transform.position,
            structure);

        Storage<Resource> resourceStorage = new(ownerInfo, _eventAggregator);
        TaskQueue<Resource> taskQueue = new(_taskFactory, _eventAggregator);
        TransportationTaskAssigner<Resource> taskAssigner = new(initialWorkers, _coroutineRunner);
        BuilderManagement builderManagement = new(
            _spendBehaviourFactory,
            _timeBehaviourFactory,
            _eventAggregator,
            ownerInfo,
            structure);
        TaskManagement<Resource> taskManagement = new(taskQueue, taskAssigner, resourceStorage, structure, _eventAggregator);

        structure.Initialize(intervalScanner, resourceStorage, _workerFactory, taskManagement, builderManagement, _eventAggregator);
        structure.EnableLogics();
    }

    private void InitializeFactories()
    {
        _taskFactory = new(c => new(c));
        _spendBehaviourFactory = new TwoParametersFactory<SpendBehaviour, ResourceSpendRequest, int>(
            (request, priority) => new(request, priority));
        _timeBehaviourFactory = new TwoParametersFactory<TimeBehaviour, TimeBehaviourContext, int>(
            (context, priority) => new(context, priority));
    }
}