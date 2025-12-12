using UnityEngine;

[CreateAssetMenu(menuName = "Homework/BuilderConfig", fileName = "New Builder config", order = 51)]
public class BuilderConfig : ScriptableObject
{
    [SerializeField] private BaseStructure _basePrefab;
    [SerializeField] private StructureBuildInfo _baseInfo;

    [SerializeField] private TransportationWorker _workerPrefab;
    [SerializeField] private StructureBuildInfo _workerInfo;

    public BaseStructure BasePrefab => _basePrefab;
    public StructureBuildInfo BaseInfo => _baseInfo;
    public TransportationWorker WorkerPrefab => _workerPrefab;
    public StructureBuildInfo WorkerInfo => _workerInfo;
}