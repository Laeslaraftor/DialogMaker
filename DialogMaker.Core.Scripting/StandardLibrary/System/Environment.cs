namespace System;

public static class Environment
{
    public static string NewLine
    {
        get
        {
            field ??= GetNewLine();
            return field;
        }
    }
    public static int TickCount => GetTickCount();

    public static string[] GetCommandLineArgs()
    {
        return Array<string>.Empty;
    }

    private static extern int GetTickCount();
    private static extern string GetNewLine();
} 