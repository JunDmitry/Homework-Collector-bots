using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Homework/ResourceSpawnerConfig", fileName = "New " + nameof(ResourceSpawnerConfig), order = 51)]
public class ResourceSpawnerConfig : ScriptableObject
{
    [SerializeField] private ResourceWeight _woodWeight;
    [SerializeField] private ResourceWeight _stoneWeight;
    [SerializeField] private ResourceWeight _ironWeight;

    [SerializeField] private List<string> _resourceAssetPaths;
    [SerializeField] private Area _spawnArea;
    [SerializeField] private float _spawnInterval;

    public ResourceWeight WoodWeight => _woodWeight;
    public ResourceWeight StoneWeight => _stoneWeight;
    public ResourceWeight IronWeight => _ironWeight;
    public IReadOnlyList<string> ResourceAssetPaths => _resourceAssetPaths.AsReadOnly();
    public Area SpawnArea => _spawnArea;
    public float SpawnInterval => _spawnInterval;
}