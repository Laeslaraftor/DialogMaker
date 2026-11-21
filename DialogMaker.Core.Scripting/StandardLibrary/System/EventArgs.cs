namespace System;

public abstract class EventArgs
{
    public static readonly EventArgs Empty = new EmptyEventArgs();

    private class EmptyEventArgs : EventArgs
    {
    }
}