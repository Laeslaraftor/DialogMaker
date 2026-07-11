namespace System;

public class NotImplementedException : Exception
{
    public NotImplementedException() : base()
    {
    }
    public NotImplementedException(string message) : base(message)
    {
    }
    public NotImplementedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}