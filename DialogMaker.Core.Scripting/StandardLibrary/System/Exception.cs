using System.Diagnostics;

namespace System;

public class Exception
{
    public Exception()
    {
        Message = string.Empty;
    }
    public Exception(string message)
    {
        Message = message;
    }
    public Exception(string message, Exception innerException)
    {
        Message = message;
        InnerException = innerException;
    }

    public string Message { get; }
    public StackTrace StackTrace { get; }
    public Exception InnerException { get; }

    public override string ToString()
    {
        return GetType().Name + ": " + Message;
    }
}