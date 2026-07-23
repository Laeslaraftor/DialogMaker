namespace System;

public static class Console
{
    public static extern void Write(string text);
    public static extern void WriteLine(string text);
    public static extern void WriteLine(char text);
    public static extern void WriteLine();
    public static extern string ReadLine();
}