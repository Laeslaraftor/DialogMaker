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

    public abstract int ReadByte();
    public abstract void Write(byte[] data, int offset, int count);

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