using System;

public static class ThrowIf
{
    public static void Null(object argument, string paramName)
    {
        if (argument == null)
            throw new ArgumentNullException(paramName);
    }

    public static void Invalid(bool condition, string message)
    {
        if (condition)
            throw new InvalidOperationException(message);
    }

    public static void Argument(bool condition, string message, string paramName)
    {
        if (condition)
            throw new ArgumentException(message, paramName);
    }

    public static void Disposed(bool condition, string objName)
    {
        if (condition)
            throw new ObjectDisposedException(objName);
    }
}