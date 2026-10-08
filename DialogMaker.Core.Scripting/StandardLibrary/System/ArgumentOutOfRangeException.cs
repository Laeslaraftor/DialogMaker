namespace System;

public class ArgumentOutOfRangeException : SystemException
{
    public ArgumentOutOfRangeException() : base()
    {
    }
    public ArgumentOutOfRangeException(string parameterName) : base("Argument value out of range: " + parameterName)
    {
        ParameterName = parameterName;
    }
    public ArgumentOutOfRangeException(string message, string parameterName) : base(message)
    {
        ParameterName = parameterName;
    }
    public ArgumentOutOfRangeException(string message, Exception innerException) : base(message, innerException)
    {
    }
    public ArgumentOutOfRangeException(string message, string parameterName, Exception innerException) : base(message, innerException)
    {
        ParameterName = parameterName;
    }

    public string? ParameterName { get; }
}