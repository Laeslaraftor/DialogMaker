namespace System.Diagnostics;

public class StackTrace
{
    private StackTrace(StackFrame[] frames)
    {
        _frames = frames;
    }

    public int FramesCount => _frames.Length;
    public StackFrame this[int index] => _frames[index];

    private readonly StackFrame[] _frames;

    public static extern StackTrace Create();
}