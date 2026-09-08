namespace System;

public class ArgumentException : Exception
{
    public ArgumentException() : base()
    {
    }
    public ArgumentException(string message) : base(message)
    {
    }
    public ArgumentException(string message, string parameterName) : base(message)
    {
        ParameterName = parameterName;
    }
    public ArgumentException(string message, Exception innerException) : base(message, innerException)
    {
    }
    public ArgumentException(string message, string parameterName, Exception innerException) : base(message, innerException)
    {
        ParameterName = parameterName;
    }

    public string? ParameterName { get; }

    public override string ToString()
    {
        var parameterName = ParameterName;
        var result = base.ToString();

        if (parameterName != null)
        {
            result += " (param: " + parameterName + ")";
        }

        return result;
    }
}