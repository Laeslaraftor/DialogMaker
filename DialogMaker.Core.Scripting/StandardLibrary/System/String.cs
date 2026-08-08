namespace System;

using System.Collections.Generic;

public sealed class String : IEnumerable<char>, IEquatable<string>
{
    public String()
    {
    }
    public String(char[] chars)
    {
    }
    public String(Span<char> chars)
    {
    }

    public int Length => GetLength();
    public char this[int index] => GetValue(index);

    public override string ToString() => this;
    public override bool Equals(object? obj)
    {
        var other = obj as string;

        if (other == null)
        {
            return false;
        }

        return Equals(other);
    }
    public bool Equals(string other)
    {
        int length = Length;

        if (other == null ||
            other.Length != length)
        {
            return false;
        }

        for (int i = 0; i < length; i++)
        {
            if (this[i] != other[i])
            {
                return false;
            }
        }

        return true;
    }

    public IEnumerator<char> GetEnumerator()
    {
        return new Enumerator(this);
    }

    public string[] Split(char separator)
    {
        int partsCount = 1;
        int length = Length;

        for (int i = 0; i < length; i++)
        {
            if (this[i] == separator)
            {
                partsCount++;
            }
        }

        string[] result = new string[partsCount];
        int currentPart = 0;
        Span<char> tempBuffer = stackalloc char[512];
        int bufferIndex = 0;

        for (int i = 0; i < length; i++)
        {
            var value = this[i];

            if (value == separator)
            {
                result[currentPart] = new(tempBuffer.Slice(0, bufferIndex + 1));
                currentPart++;
                bufferIndex = 0;
                continue;
            }

            tempBuffer[bufferIndex] = value;
            bufferIndex++;

            if (bufferIndex >= tempBuffer.Length)
            {
                int tempBufferLength = tempBuffer.Length;
                char[] newBuffer = new char[tempBufferLength * 2];
                
                for (int b = 0; b < tempBufferLength; i++)
                {
                    newBuffer[b] = tempBuffer[b];
                }

                tempBuffer = newBuffer;
            }
        }

        result[currentPart] = new(tempBuffer.Slice(0, bufferIndex + 1));

        return result;
    }
    public string Replace(string oldValue, string newValue)
    {
        int length = Length;

        if (oldValue == null ||
            oldValue == newValue ||
            oldValue.Length > length)
        {
            return this;
        }

        int oldValueLength = oldValue.Length;
        int newValueLength = newValue.Length;
        int newLength = 0;
        int index = 0;

        while (index < length)
        {
            bool match = true;

            for (int v = 0; v < oldValueLength; v++)
            {
                if (oldValue[v] != this[index + v])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                index += oldValueLength;
                newLength += newValueLength;
                continue;
            }

            index++;
            newLength++;
        }

        Span<char> buffer = newLength > 1024 ? new char[newLength] : stackalloc char[newLength];
        index = 0;
        newLength = 0;

        while (index < length)
        {
            bool match = true;

            for (int v = 0; v < oldValueLength; v++)
            {
                if (oldValue[v] != this[index + v])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                for (int v = 0; v < newValueLength; v++)
                {
                    buffer[newLength + v] = newValue[v];
                }

                index += oldValueLength;
                newLength += newValueLength;
                continue;
            }

            buffer[newLength] = this[index];
            index++;
            newLength++;
        }

        return new(buffer);
    }

    private extern int GetLength();
    private extern char GetValue(int index);

    public static string operator +(string l, string r) => Ctor(l, r);
    public static string operator +(string l, object r) => Ctor(l, r.ToString());
    public static string operator +(object l, string r) => Ctor(l.ToString(), r);

    public static readonly string Empty = "";

    public static bool IsNullOrEmpty(string str) => str == null || str.Length == 0;

    private static string Ctor() => Empty;
    private static extern string Ctor(char[] chars);
    private static extern string Ctor(Span<char> chars);
    private static extern string Ctor(string str1, string str2);
    private static extern string Ctor(string[] values);

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