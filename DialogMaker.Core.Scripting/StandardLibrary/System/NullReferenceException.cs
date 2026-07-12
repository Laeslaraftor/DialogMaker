namespace System;

public class NullReferenceException : Exception
{
    public NullReferenceException() : base()
    {
    }
    public NullReferenceException(string message) : base(message)
    {
    }
    public NullReferenceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}