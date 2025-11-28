using System;

public interface ICollectedEvent<T>
{
    event Action<T> Collected;
}