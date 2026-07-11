namespace System;

public class ArgumentException : Exception
{
    public ArgumentException() : base()
    {
    }
    public ArgumentException(string message) : base(message)
    {
    }
    public ArgumentException(string message, Exception innerException) : base(message, innerException)
    {
    }
}