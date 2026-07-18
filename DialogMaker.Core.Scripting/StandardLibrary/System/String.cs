namespace System;

using System.Collections.Generic;

public sealed class String : IEnumerable<char>
{
    public String()
    {
    }
    public String(char[] chars) : this(chars, true)
    {
    }
    private String(char[] chars, bool makeCopy)
    {
    }

    public int Length => GetLength();
    public char this[int index] => GetValue(index);

    public override string ToString() => this;

    public IEnumerator<char> GetEnumerator()
    {
        return new Enumerator(this);
    }

    private extern int GetLength();
    private extern char GetValue(int index);

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

    private class Enumerator : IEnumerator<char>
    {
        public Enumerator(string str)
        {
            _str = str;
        }

        public char Current { get; private set; }

        private readonly string _str;
        private int _currentIndex = -1;

        public bool MoveNext()
        {
            if (_currentIndex + 1 >= _str.Length)
            {
                return false;
            }

            _currentIndex++;
            Current = _str[_currentIndex];

            return true;
        }
        public void Reset()
        {
            Current = '\0';
            _currentIndex = -1;
        }
    }
}