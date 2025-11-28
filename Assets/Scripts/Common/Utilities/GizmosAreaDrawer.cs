using UnityEngine;

public class GizmosAreaDrawer : MonoBehaviour
{
    [SerializeField] private ResourceSpawnerConfig _spawnerConfigToDraw;
    [SerializeField] private Vector3 _spawnerPosition;

    private void OnDrawGizmos()
    {
        if (_spawnerConfigToDraw == null
            || _spawnerConfigToDraw.SpawnArea == null)
            return;

        Gizmos.color = Color.red * new Color(1, 1, 1, .5f);

        foreach (Rectangle rectangle in _spawnerConfigToDraw.SpawnArea)
            Gizmos.DrawCube(rectangle.Bounds.center + _spawnerPosition, rectangle.Bounds.size);
    }
}