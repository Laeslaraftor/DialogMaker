using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting
{
    internal unsafe static class UnmanagedExtensions
    {
        extension(Stream stream)
        {
            public void Write<T>(T value) where T : unmanaged
            {
                DSharpStream dStream = stream;
                dStream.Write(value);
            }
            public void WriteString(string value)
            {
                DSharpStream dStream = stream;
                dStream.WriteString(value);
            }

            public T Read<T>() where T : unmanaged
            {
                DSharpStream dStream = stream;
                return dStream.Read<T>();
            }
            public string ReadString()
            {
                DSharpStream dStream = stream;
                return dStream.ReadString();
            }
        }
        extension(DSharpStream stream)
        {
            public void Write<T>(T value) where T : unmanaged
            {
                var bytes = (byte*)&value;
                var size = sizeof(T);

                for (int i = 0; i < size; i++)
                {
                    stream.WriteByte(bytes[i]);
                }
            }
            public void WriteString(string value)
            {
                stream.Write(value.Length);

                if (value.Length == 0)
                {
                    return;
                }

                var size = value.Length * sizeof(char);

                fixed (char* chars = value)
                {
                    var charsBytes = (byte*)chars;
                    stream.Write(new(charsBytes, size));
                }
            }

            public T Read<T>() where T : unmanaged
            {
                var size = sizeof(T);
                T* item = stackalloc T[1];

                var bytesRead = stream.Read(new(item, size));

                if (bytesRead != size)
                {
                    throw new InvalidOperationException("Unable to read value");
                }

                return *item;
            }
            public string ReadString()
            {
                var length = stream.Read<int>();

                if (length == 0)
                {
                    return string.Empty;
                }

                int charsBufferSize = length * sizeof(char);

                void Read(Span<byte> buffer)
                {
                    var read = stream.Read(buffer);

                    if (read != buffer.Length)
                    {
                        throw new InvalidOperationException("Unable to read string chars");
                    }
                }

                if (charsBufferSize < 512 * 1024)
                {
                    char* charsBuffer = stackalloc char[length];
                    stream.Read(new(charsBuffer, charsBufferSize));

                    return new string(charsBuffer, 0, length);
                }
                else
                {
                    byte[] charsBuffer = new byte[charsBufferSize];
                    Read(new(charsBuffer));

                    fixed (byte* chars = charsBuffer)
                    {
                        return new string((char*)chars, 0, length);
                    }
                }
            }
        }
    }
}
