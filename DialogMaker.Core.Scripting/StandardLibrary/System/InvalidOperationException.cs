namespace System;

public class InvalidOperationException : Exception
{
    public InvalidOperationException() : base("Invalid operation")
    {
    }
    public InvalidOperationException(string message) : base(message)
    {
    }
    public InvalidOperationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}