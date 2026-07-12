namespace System.Diagnostics;

public class StackFrame
{
    public string FilePath { get; }
    public int Line { get; }
    public int Column { get; }
}