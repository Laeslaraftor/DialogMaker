namespace System;

public static class Environment
{
    public static readonly string NewLine = GetNewLine();
    public static int TickCount => GetTickCount();

    public static string[] GetCommandLineArgs()
    {
        return Array<string>.Empty;
    }

    private static extern int GetTickCount();
    private static extern string GetNewLine();
} 