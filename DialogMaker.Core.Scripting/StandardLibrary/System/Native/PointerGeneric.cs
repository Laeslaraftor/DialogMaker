namespace System.Native;

public struct Pointer<T> where T : struct
{
    public Pointer(nint address)
    {
        _address = address;
    }

    public T this[int offset]
    {
        get => Pointer.Read<T>(_address + sizeof(T) * offset);
        set => Pointer.Write(_address + sizeof(T) * offset, value);
    }

    private readonly nint _address;

    public TValue Read<TValue>() where TValue : struct
    {
        return Pointer.Read<TValue>(_address);
    }
    public TValue Read<TValue>(int offsetInBytes) where TValue : struct
    {
        return Pointer.Read<TValue>(_address + offsetInBytes);
    }
    public void Write<TValue>(TValue value) where TValue : struct
    {
        Pointer.Write(_address, value);
    }
    public void Write<TValue>(int offsetInBytes, TValue value) where TValue : struct
    {
        Pointer.Write(_address + offsetInBytes, value);
    }

    public static implicit operator Pointer<T>(nint address) => new Pointer<T>(address);
    public static implicit operator nint(Pointer<T> pointer) => pointer._address;
    public static Pointer<T> operator +(Pointer<T> pointer, long offset) => new Pointer<T>(pointer._address + offset);
    public static Pointer<T> operator -(Pointer<T> pointer, long offset) => new Pointer<T>(pointer._address - offset);
}