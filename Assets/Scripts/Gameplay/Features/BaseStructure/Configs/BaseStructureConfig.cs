using UnityEngine;

[CreateAssetMenu(menuName = "Homework/BaseStructureConfig", fileName = "New BaseStructure config", order = 51)]
public class BaseStructureConfig : ScriptableObject
{
    [SerializeField] private Area _waitArea;
    [SerializeField] private Area _shipmentArea;
    [SerializeField] private ScanInfo _scanInfo;
    [SerializeField] private int _initialWorkerCount;
    [SerializeField] private float _minShipmentDistance;
    [SerializeField] private float _taskAssignInterval;

    public Area WaitArea => _waitArea;
    public Area ShipmentArea => _shipmentArea;
    public ScanInfo ScanInfo => _scanInfo;
    public int InitialWorkerCount => _initialWorkerCount;
    public float MinShipmentDistance => _minShipmentDistance;
    public float TaskAssignInterval => _taskAssignInterval;
}