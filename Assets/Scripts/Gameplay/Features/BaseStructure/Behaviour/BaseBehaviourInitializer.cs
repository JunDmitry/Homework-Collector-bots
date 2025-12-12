using System.Collections.Generic;
using UnityEngine;

public class BaseBehaviourInitializer
{
    private readonly BaseStructureInitializeContext _initializeContext;
    private readonly IInstantiator _instantiator;

    public BaseBehaviourInitializer(BaseStructureInitializeContext initializeContext, IInstantiator instantiator = null)
    {
        ThrowIf.Null(initializeContext, nameof(initializeContext));

        _initializeContext = initializeContext;
        _instantiator = instantiator ?? new ObjectInstantiator();
    }

    public BehaviourAddContext<ITransportationWorker<Resource>> CreateInitialWorkerContext()
    {
        ITwoParametersFactory<ITransportationWorker<Resource>, TransportationWorker, Vector3> workerFactory = _initializeContext.WorkerFactory;
        BuilderConfig builderConfig = _initializeContext.BuilderConfig;
        Storage<Resource> storage = _initializeContext.Storage;

        BuildBehaviourContext<ITransportationWorker<Resource>> buildBehaviourContext = new(
                new Factory<ITransportationWorker<Resource>>(() => workerFactory.Create(builderConfig.WorkerPrefab, _initializeContext.WaitPosition)),
                new(storage, new()));
        TimeBehaviourContext timeBehaviourContext = new();

        return new(buildBehaviourContext, timeBehaviourContext);
    }

    public BehaviourAddContext<ITransportationWorker<Resource>> CreateConstantWorkerContext()
    {
        BuilderConfig builderConfig = _initializeContext.BuilderConfig;
        Storage<Resource> storage = _initializeContext.Storage;
        BaseAnimator animator = _initializeContext.Animator;

        IEmptyFactory<ITransportationWorker<Resource>> constantFactory = new Factory<ITransportationWorker<Resource>>(
            () => _instantiator.Instantiate(builderConfig.WorkerPrefab, _initializeContext.WaitPosition, Quaternion.identity));
        BuildBehaviourContext<ITransportationWorker<Resource>> buildBehaviourConstant = new(
            constantFactory, new(storage, builderConfig.WorkerInfo.SpendContext));
        TimeBehaviourContext timeBehaviourConstant = new(
            makeAnimation: () => animator.WaitWhileBuild(builderConfig.WorkerInfo.BuildTimeInSeconds));

        return new(buildBehaviourConstant, timeBehaviourConstant);
    }

    public BehaviourAddContext<BaseStructure> CreateColonizationContext()
    {
        BuilderConfig builderConfig = _initializeContext.BuilderConfig;
        Storage<Resource> storage = _initializeContext.Storage;
        FlagPlacer flagPlacer = _initializeContext.FlagPlacer;
        TaskManagement<Resource> taskManagement = _initializeContext.TaskManagement;
        BaseAnimator animator = _initializeContext.Animator;
        BaseStructureInitializer structureInitializer = new(
            _initializeContext.EventAggregator,
            _initializeContext.CoroutineRunner,
            _initializeContext.WorkerFactory);

        float prepareTime = builderConfig.BaseInfo.BuildTimeInSeconds * .3f;
        int priority = (int)BaseBehaviourPriority.VeryLow + 1;
        int minimumTotalWorkersCount = 2;

        InnerBehaviour innerBehaviour = new(
            new SpendBehaviour(
                new ResourceSpendRequest(storage, builderConfig.BaseInfo.SpendContext),
                ((int)BaseBehaviourPriority.VeryHigh) + 1),
            (с) => flagPlacer.Placed && taskManagement.TotalCountWorkers >= minimumTotalWorkersCount);

        TimeBehaviourContext baseTimeContext = new(makeAnimation: () => animator.WaitWhileBuild(prepareTime));
        DelegateBehaviour lockBehaviour = new ActionBehaviour(
            c => flagPlacer.LockPosition(),
            priority: priority--);
        BuildBehaviourContext<BaseStructure> baseBuildContext = new(
            new Factory<BaseStructure>(() => _instantiator.Instantiate(builderConfig.BasePrefab, flagPlacer.FlagPosition, Quaternion.identity)),
            new ResourceSpendRequest(storage, new()));
        priority--;
        ScaleSetterBehaviour<BaseStructure> scaleSetter = new(Vector3.right, priority--);
        GetterWorkerBehaviour<Resource> workerBehaviour = new(
            taskManagement.TakeFreeWorkerRoutine,
            priority: priority--);
        WorkerMoverToStructureBehaviour<Resource> workerMoverTo = new(priority--);
        ScaleAnimationToInitialBehaviour<BaseStructure> scaleAnimation = new(
            builderConfig.BaseInfo.BuildTimeInSeconds,
            priority: priority--);
        BaseStructureInitializeBehaviour structureInitializeBehaviour = new(
            structureInitializer,
            priority: priority--);
        DelegateBehaviour unlockBehaviour = new ActionBehaviour(
            c =>
            {
                flagPlacer.UnlockPosition();
                flagPlacer.TryUnplace();
            },
            priority: priority--);

        List<IBaseBehaviour> additionalBehaviour = new()
        {
            innerBehaviour,
            lockBehaviour,
            scaleSetter,
            workerBehaviour,
            workerMoverTo,
            scaleAnimation,
            structureInitializeBehaviour,
            unlockBehaviour
        };

        return new(baseBuildContext, baseTimeContext, additionalBehaviour);
    }
}