using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AssetProviderExtensions
{
    public static PairWeight<GameObject> LoadWithWeight(this IAssetProvider assetProvider, string path, Func<GameObject, float> getWeight)
    {
        ThrowIf.Null(getWeight, nameof(getWeight));

        GameObject gameObject = assetProvider.Load(path);
        float weight = getWeight.Invoke(gameObject);

        return PairWeight<GameObject>.Create(gameObject, weight);
    }

    public static PairWeight<T> LoadWithWeight<T>(this IAssetProvider assetProvider, string path, Func<T, float> getWeight)
        where T : Component
    {
        ThrowIf.Null(getWeight, nameof(getWeight));

        T item = assetProvider.Load<T>(path);
        float weight = getWeight.Invoke(item);

        return PairWeight<T>.Create(item, weight);
    }

    public static IEnumerable<PairWeight<GameObject>> LoadAllWithWeight(
        this IAssetProvider assetProvider,
        IEnumerable<string> paths,
        Func<GameObject, float> getWeight)
    {
        ThrowIf.Null(getWeight, nameof(getWeight));

        return LoadAll(assetProvider, paths)
            .Select(a => PairWeight<GameObject>.Create(a, getWeight.Invoke(a)));
    }

    public static IEnumerable<PairWeight<T>> LoadAllWithWeight<T>(
        this IAssetProvider assetProvider,
        IEnumerable<string> paths,
        Func<T, float> getWeight)
        where T : Component
    {
        ThrowIf.Null(getWeight, nameof(getWeight));

        return LoadAll<T>(assetProvider, paths)
            .Select(a => PairWeight<T>.Create(a, getWeight.Invoke(a)));
    }

    public static IEnumerable<GameObject> LoadAll(this IAssetProvider assetProvider, IEnumerable<string> paths)
    {
        return paths.Select(assetProvider.Load);
    }

    public static IEnumerable<T> LoadAll<T>(this IAssetProvider assetProvider, IEnumerable<string> paths)
        where T : Component
    {
        return paths.Select(assetProvider.Load<T>);
    }
}