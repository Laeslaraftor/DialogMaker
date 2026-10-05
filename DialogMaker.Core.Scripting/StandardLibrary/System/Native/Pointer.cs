namespace System.Native;

public struct Pointer
{
    public Pointer(nint address)
    {
        _address = address;
    }

    public bool IsNull => _address == 0;

    private readonly nint _address;

    public static explicit operator Pointer(nint address) => new Pointer(address);
    public static explicit operator nint(Pointer pointer) => pointer._address;
    public static Pointer operator +(Pointer pointer, long offset) => new Pointer(pointer._address + offset);
    public static Pointer operator -(Pointer pointer, long offset) => new Pointer(pointer._address - offset);
}