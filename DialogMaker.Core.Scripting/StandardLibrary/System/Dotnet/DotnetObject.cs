namespace System.Dotnet;

/// <summary>
/// Base class for .NET object handlers and wrappers.
/// </summary>
public abstract class DotnetObject : IDisposable
{
    protected DotnetObject(string typeName)
    {
        bool isCreated = CreateDotnetObject(this, typeName);

        if (!isCreated)
        {
            throw new ArgumentException("Unable to create .NET object instance of \"" + typeName + "\"", nameof(typeName));
        }
    }
    ~DotnetObject()
    {
        Dispose(false);
    }

    public event EventHandler? Disposed;

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            IsDisposed = true;
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        DisposeDotnetObject(this);
    }

    private static extern bool CreateDotnetObject(DotnetObject instance, string typeName);
    private static extern void DisposeDotnetObject(DotnetObject instance);
}