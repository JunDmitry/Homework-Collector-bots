using System;
using System.Collections.Generic;
using System.Linq;

public static class Utils
{
    private static readonly ItemDeliverableType[] s_resourceTypes = Enum
        .GetValues(typeof(ItemDeliverableType))
        .Cast<ItemDeliverableType>()
        .Except(new ItemDeliverableType[] { ItemDeliverableType.Unknown })
        .ToArray();

    public static Dictionary<ItemDeliverableType, int> GenerateEmptyDeliverableMap()
    {
        Dictionary<ItemDeliverableType, int> resourceMap = new(s_resourceTypes.Length);

        foreach (ItemDeliverableType resourceType in s_resourceTypes)
            resourceMap[resourceType] = 0;

        return resourceMap;
    }

    public static int FindIndexWithLessOrEqual<T>(this IList<T> values, T maxValue)
        where T : IComparable<T>
    {
        int left = 0;
        int right = values.Count - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;

            if (values[mid].CompareTo(maxValue) <= 0)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return right;
    }

    public static Dictionary<ItemDeliverableType, ResourceWeight> GetWeightsAsDictionary(this ResourceSpawnerConfig config)
    {
        Dictionary<ItemDeliverableType, ResourceWeight> weightsMap = new()
        {
            { ItemDeliverableType.Wood, config.WoodWeight },
            { ItemDeliverableType.Stone, config.StoneWeight },
            { ItemDeliverableType.Iron, config.IronWeight },
        };

        return weightsMap;
    }
}