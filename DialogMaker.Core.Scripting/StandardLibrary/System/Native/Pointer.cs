namespace System.Native;

public struct Pointer
{
    public Pointer(nint address)
    {
        _address = address;
    }

    public bool IsNull => _address == 0;

    private readonly nint _address;

    public T Read<T>() where T : struct
    {
        return ReadValue<T>(_address);
    }
    public T Read<T>(int offsetInBytes) where T : struct
    {
        return ReadValue<T>(_address + offsetInBytes);
    }
    public void Write<T>(T value) where T : struct
    {
        WriteValue(_address, value);
    }
    public void Write<T>(int offsetInBytes, T value) where T : struct
    {
        WriteValue(_address + offsetInBytes, value);
    }

    public static implicit operator Pointer(nint address) => new Pointer(address);
    public static implicit operator nint(Pointer pointer) => pointer._address;
    public static Pointer operator +(Pointer pointer, long offset) => new Pointer(pointer._address + offset);
    public static Pointer operator -(Pointer pointer, long offset) => new Pointer(pointer._address - offset);

    internal static extern T ReadValue<T>(nint address);
    internal static extern void WriteValue<T>(nint address, T value);
}