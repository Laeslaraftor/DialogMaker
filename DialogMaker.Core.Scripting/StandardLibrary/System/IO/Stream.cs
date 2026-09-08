namespace System.IO;

public abstract class Stream : IDisposable
{
    ~Stream()
    {
        Dispose(false);
    }

    public abstract long Position { get; set; }
    public abstract long Length { get; }
    public abstract bool CanWrite { get; }
    public abstract bool CanRead { get; }
    public abstract bool CanTimeout { get; }
    public virtual int WriteTimeout { get; set; }
    public virtual int ReadTimeout { get; set; }

    public virtual int ReadByte()
    {
        Span<byte> buffer = stackalloc byte[1];
        int bytesRead = Read(buffer);

        if (bytesRead == 0)
        {
            return -1;
        }

        return buffer[0];
    }
    public abstract int Read(byte[] buffer, int offset, int count);
    public abstract int Read(Span<byte> buffer);
    public abstract void Write(byte[] data, int offset, int count);
    public abstract void Write(Span<byte> data);

    public void Dispose() => Close();
    public void Close()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}