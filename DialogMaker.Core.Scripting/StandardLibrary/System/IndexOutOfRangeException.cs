namespace System;

public class IndexOutOfRangeException : Exception
{
    public IndexOutOfRangeException() : base()
    {
    }
    public IndexOutOfRangeException(string message) : base(message)
    {
    }
    public IndexOutOfRangeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}