using System.Dotnet;

namespace System.Diagnostics;

public class Stopwatch : DotnetObject
{
    public Stopwatch() : base(_typeName)
    {
    }

    public bool IsRunning => GetIsRunning();
    public TimeSpan Elapsed => new(GetElapsedTicks());
    public long ElapsedMilliseconds => GetElapsedMilliseconds();
    public long ElapsedTicks => GetElapsedTicks();

    public extern void Start();
    public extern void Restart();
    public extern void Reset();
    public extern void Stop();

    private extern bool GetIsRunning();
    private extern long GetElapsedTicks();
    private extern long GetElapsedMilliseconds();

    public static readonly long Frequency = GetFrequency();
    public static readonly bool IsHighResolution = GetIsHighResolution();

    private static readonly string _typeName = "System.Diagnostics.Stopwatch";

    public static extern long GetTimestamp();
    public static Stopwatch StartNew()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        return stopwatch;
    }

    private static extern long GetFrequency();
    private static extern bool GetIsHighResolution();
}