namespace System;

public static class Console
{
    public static extern void Write(string text);
    public static extern void WriteLine(string text);
    public static extern void WriteLine(char text);
    public static void WriteLine(object? obj)
    {
        if (obj == null)
        {
            WriteLine();
            return;
        }

        WriteLine(obj.ToString());
    }
    public static extern void WriteLine();
    public static extern string ReadLine();
    public static extern void Clear();
}