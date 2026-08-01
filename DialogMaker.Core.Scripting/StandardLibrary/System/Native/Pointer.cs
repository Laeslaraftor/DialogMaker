namespace System.Native;

public struct Pointer
{
    public Pointer(nint address)
    {
        _address = address;
    }

    private readonly nint _address;

    public T Read<T>() where T : struct
    {
        return Read<T>(_address);
    }
    public T Read<T>(int offsetInBytes) where T : struct
    {
        return Read<T>(_address + offsetInBytes);
    }
    public void Write<T>(T value) where T : struct
    {
        Write(_address, value);
    }
    public void Write<T>(int offsetInBytes, T value) where T : struct
    {
        Write(_address + offsetInBytes, value);
    }

    public static implicit operator Pointer(nint address) => new Pointer(address);
    public static implicit operator nint(Pointer pointer) => pointer._address;
    public static Pointer operator +(Pointer pointer, long offset) => new Pointer(pointer._address + offset);
    public static Pointer operator -(Pointer pointer, long offset) => new Pointer(pointer._address - offset);

    internal static extern T Read<T>(nint address);
    internal static extern void Write<T>(nint address, T value);
}