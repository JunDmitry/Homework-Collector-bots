using UnityEngine;

public interface IAssetProvider
{
    GameObject Load(string path);
    T Load<T>(string path) where T : Component;
}