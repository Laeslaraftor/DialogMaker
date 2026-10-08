namespace System;

public class ArithmeticException : SystemException
{
    public ArithmeticException() : base("Arithmetic exception was coughed.")
    {
    }
    public ArithmeticException(string message) : base(message)
    {
    }
    public ArithmeticException(string message, Exception innerException) : base(message, innerException)
    {
    }
}