using System.Collections.Generic;

public static class FactoryExtensions
{
    public static IEnumerable<T> CreateCount<T>(this IEmptyFactory<T> factory, int count)
        where T : class
    {
        ThrowIf.Argument(count <= 0, $"{nameof(count)} units for create in {nameof(Factory<T>)} must be positive", nameof(count));

        for (int i = 0; i < count; i++)
            yield return factory.Create();
    }
}