using UnityEngine;

public class AssetProvider : IAssetProvider
{
    public GameObject Load(string path)
    {
        return Resources.Load<GameObject>(path);
    }

    public T Load<T>(string path)
        where T : Component
    {
        return Resources.Load<T>(path);
    }
}