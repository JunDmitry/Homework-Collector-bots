using UnityEngine;

public class BaseStructureInitializeContext
{
    public BaseStructureInitializeContext(
        ICoroutineRunner coroutineRunner,
        BaseStructureConfig structureConfig,
        ITwoParametersFactory<ITransportationWorker<Resource>, TransportationWorker, Vector3> workerFactory,
        BuilderConfig builderConfig,
        Storage<Resource> storage,
        BaseAnimator animator,
        IEventAggregator eventAggregator,
        FlagPlacer flagPlacer,
        TaskManagement<Resource> taskManagement,
        OwnerInfo ownerInfo)
    {
        CoroutineRunner = coroutineRunner;
        StructureConfig = structureConfig;
        WorkerFactory = workerFactory;
        BuilderConfig = builderConfig;
        Storage = storage;
        Animator = animator;
        EventAggregator = eventAggregator;
        FlagPlacer = flagPlacer;
        TaskManagement = taskManagement;
        OwnerInfo = ownerInfo;
    }

    public ICoroutineRunner CoroutineRunner { get; }
    public BaseStructureConfig StructureConfig { get; }
    public ITwoParametersFactory<ITransportationWorker<Resource>, TransportationWorker, Vector3> WorkerFactory { get; }
    public BuilderConfig BuilderConfig { get; }
    public Storage<Resource> Storage { get; }
    public BaseAnimator Animator { get; }
    public IEventAggregator EventAggregator { get; }
    public FlagPlacer FlagPlacer { get; }
    public TaskManagement<Resource> TaskManagement { get; }
    public OwnerInfo OwnerInfo { get; }

    public Vector3 WaitPosition => StructureConfig.WaitArea.RandomPosition() + OwnerInfo.Position;
    public Vector3 ShipmentPosition => StructureConfig.ShipmentArea.RandomPosition() + OwnerInfo.Position;
    public float MinShipmentDistance => StructureConfig.MinShipmentDistance;
    public float TaskAssignInterval => StructureConfig.TaskAssignInterval;
}