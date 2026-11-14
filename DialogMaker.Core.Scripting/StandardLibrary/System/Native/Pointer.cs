namespace System.Native;

public unsafe struct Pointer
{
    public Pointer(nint address)
    {
        _address = address;
    }

    private readonly nint _address;

    public static explicit operator Pointer(nint address) => new(address);
    public static explicit operator nint(Pointer pointer) => pointer._address;
    public static Pointer operator +(Pointer pointer, long offset) => new(pointer._address + offset);
    public static Pointer operator -(Pointer pointer, long offset) => new(pointer._address - offset);
}