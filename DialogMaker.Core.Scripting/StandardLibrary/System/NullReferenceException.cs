namespace System;

public class NullReferenceException : Exception
{
    public NullReferenceException() : base("Object reference not references to object instance")
    {
    }
    public NullReferenceException(string message) : base(message)
    {
    }
    public NullReferenceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}