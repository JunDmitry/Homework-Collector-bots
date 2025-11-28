using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour, ICoroutineRunner
{
    private const float MinInterval = .1f;

    [SerializeField] private BaseStructure _baseStructure;
    [SerializeField] private TransportationWorker _baseWorkerPrefab;
    [SerializeField] private World _world;
    [SerializeField] private ResourceSpawnerConfig _resourceSpawnerConfig;

    [SerializeField, Min(0)] private float _scanRadius;
    [SerializeField, Min(MinInterval)] private float _scanInterval = MinInterval;

    [SerializeField] private StorageView _storageViewTemplate;
    [SerializeField] private ResourceTaskManagementView _resourceTaskManagementViewTemplate;

    private AssetProvider _assetProvider;
    private StaticDataProvider _staticDataProvider;
    private SingleParameterFactory<ObjectPool<Resource>, Resource> _resourcePoolFactory;
    private ResourceSpawner _resourceSpawner;

    private readonly List<IPresenter> _presenters = new();

    private void Awake()
    {
        InitializeInfrastructure();
        InitializeFactories();
        InitializeGameplay();
        InitializeBase();
        InitializeWorld();
    }

    private void OnEnable()
    {
        _baseStructure.EnableLogics();
        _world.BeginLifetime();

        _presenters.ForEach(p => p.Show());
    }

    private void OnDisable()
    {
        _baseStructure.DisableLogics();
        _world.StopLifetime();
        _presenters.ForEach(p => p.Hide());
    }

    private void OnDestroy()
    {
        _presenters.ForEach(p => p.Dispose());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow * new Color(1, 1, 1, .1f);
        Gizmos.DrawSphere(transform.position, _scanRadius);
    }

    private void InitializeInfrastructure()
    {
        _assetProvider = new AssetProvider();
        _staticDataProvider = new StaticDataProvider();

        _staticDataProvider.LoadAll();
    }

    private void InitializeFactories()
    {
        _resourcePoolFactory = new(r => new(() => Instantiate(r)));
    }

    private void InitializeGameplay()
    {
        _resourceSpawner = new ResourceSpawner(_resourceSpawnerConfig, this);
        _resourceSpawner.Initialize(_assetProvider, _resourcePoolFactory);
        _resourceSpawner.Enable();
    }

    private void InitializeBase()
    {
        IScanner scanner = new Scanner(_scanRadius);
        IntervalScanner<Resource> intervalScanner = new(
            scanner, _scanInterval,
            () => _baseStructure.transform.position,
            _baseStructure);

        SingleParameterFactory<TransportationTask<Resource>, DeliveryContext<Resource>> taskFactory = new(c => new(c));
        TaskQueue<Resource> taskQueue = new(taskFactory);
        TransportationTaskAssigner<Resource> taskAssigner = new(_baseStructure);
        Storage<Resource> resourceStorage = new();
        Factory<ITransportationWorker<Resource>> workerFactory = new(
            () => CreateTransportationWorker(_baseWorkerPrefab));

        TaskManagement<Resource> taskManagement = new(taskQueue, taskAssigner, resourceStorage, _baseStructure);

        _baseStructure.Initialize(intervalScanner, resourceStorage, workerFactory, taskManagement);
        _baseStructure.EnableLogics();

        InitializeBaseUI(resourceStorage, taskManagement);
    }

    private void InitializeWorld()
    {
        _world.Initialize(_resourceSpawner);
    }

    private void InitializeBaseUI(Storage<Resource> storage, TaskManagement<Resource> taskManagement)
    {
        StorageView storageView = Instantiate(_storageViewTemplate, _baseStructure.transform);
        ResourceTaskManagementView taskManagementView = Instantiate(_resourceTaskManagementViewTemplate, _baseStructure.transform);

        ResourceStoragePresenter resourcePresenter = new(storage, storageView);
        ResourceTaskManagementPresenter resourceTaskManagementPresenter = new(taskManagementView, taskManagement);

        resourcePresenter.Initialize();
        resourceTaskManagementPresenter.Initialize();

        _presenters.Add(resourcePresenter);
        _presenters.Add(resourceTaskManagementPresenter);
    }

    private ITransportationWorker<Resource> CreateTransportationWorker(TransportationWorker prefab)
    {
        TransportationWorker instance = Instantiate(prefab, _baseStructure.WaitArea, Quaternion.identity);

        return instance;
    }
}