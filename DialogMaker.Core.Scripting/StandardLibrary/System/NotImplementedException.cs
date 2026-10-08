namespace System;

public class NotImplementedException : SystemException
{
    public NotImplementedException() : base("Type or member not implemented yet")
    {
    }
    public NotImplementedException(string message) : base(message)
    {
    }
    public NotImplementedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}