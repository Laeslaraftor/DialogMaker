namespace System;

public static class Activator
{
    public static T CreateInstance<T>()
    {
        throw new NotImplementedException();
    }
    public static object CreateInstance(Type objectType, params object[] consturctorParameters)
    {
        Console.WriteLine("Creating new instance with " + consturctorParameters.Length + " parameters");
        throw new NotImplementedException();
    }
}