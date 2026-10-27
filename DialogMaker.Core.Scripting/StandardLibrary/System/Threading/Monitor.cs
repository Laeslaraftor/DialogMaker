namespace System.Threading;

public static class Monitor
{
    public static extern void Enter(object obj);
    public static extern void Exit(object obj);
}