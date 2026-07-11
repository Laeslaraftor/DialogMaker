namespace System;

using System.Collections.Generic;

public sealed class String : IEnumerable<char>
{
    public String()
    {
        _chars = Array<char>.Empty;
    }
    public String(char[] chars) : this(chars, true)
    {
    }
    private String(char[] chars, bool makeCopy)
    {
        if (makeCopy)
        {
            _chars = new char[chars.Length];
            Array<char>.Copy(chars, _chars);
        }
        else
        {
            _chars = chars;
        }
    }

    public int Length => _chars.Length;

    private readonly char[] _chars;

    public override string ToString() => this;

    public IEnumerator<char> GetEnumerator()
    {
        return _chars.GetEnumerator();
    }

    public static string operator +(string l, string r)
    {
        char[] chars = new chars[l.Length + r.Length];
        
        for (int i = 0; i < l.Length; i++)
        {
            chars[i] = l[i];
        }
        for (int i = 0; i < r.Length; i++)
        {
            chars[i + l.Length] = r[i];
        }

        return new(chars);
    }
    public static string operator +(string l, object r) => l + r.ToString();
    public static string operator +(object l, string r) => l.ToString() + r;

    public static readonly string Empty = "";

    public static bool IsNullOrEmpty(string str) => str == null || str == Empty;
}