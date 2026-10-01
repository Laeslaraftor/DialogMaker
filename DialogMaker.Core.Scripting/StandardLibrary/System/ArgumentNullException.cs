namespace System;

public class ArgumentNullException : Exception
{
    public ArgumentNullException() : base()
    {
    }
    public ArgumentNullException(string parameterName) : base("Parameter can not be null: " + parameterName)
    {
        ParameterName = parameterName;
    }
    public ArgumentNullException(string message, string parameterName) : base(message)
    {
        ParameterName = parameterName;
    }
    public ArgumentNullException(string message, Exception innerException) : base(message, innerException)
    {
    }
    public ArgumentNullException(string message, string parameterName, Exception innerException) : base(message, innerException)
    {
        ParameterName = parameterName;
    }

    public string? ParameterName { get; }

    public static void ThrowIfNull(object? value, string parameterName)
    {
        if (value == null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }
}