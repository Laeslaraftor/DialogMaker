namespace System.Native;

public unsafe struct Pointer<T>
{
    public Pointer(nint address)
    {
        _address = address;
    }

    public T this[int offset]
    {
        get => throw new InvalidOperationException("Indexer getter was implemented by compiler");
        set => throw new InvalidOperationException("Indexer setter was implemented by compiler");
    }

    private readonly nint _address;
    
    public static explicit operator Pointer<T>(nint address) => new Pointer<T>(address);
    public static explicit operator Pointer<T>(Pointer pointer) => new Pointer<T>((nint)pointer);
    public static explicit operator Pointer(Pointer<T> pointer) => new Pointer(pointer._address);
    public static explicit operator nint(Pointer<T> pointer) => pointer._address;

    public static Pointer<T> operator +(Pointer<T> pointer, byte offset) => new Pointer<T>(pointer._address + offset * sizeof(T));
    public static Pointer<T> operator -(Pointer<T> pointer, byte offset) => new Pointer<T>(pointer._address - offset * sizeof(T));
    public static Pointer<T> operator +(Pointer<T> pointer, short offset) => new Pointer<T>(pointer._address + offset * sizeof(T));
    public static Pointer<T> operator -(Pointer<T> pointer, short offset) => new Pointer<T>(pointer._address - offset * sizeof(T));
    public static Pointer<T> operator +(Pointer<T> pointer, int offset) => new Pointer<T>(pointer._address + offset * sizeof(T));
    public static Pointer<T> operator -(Pointer<T> pointer, int offset) => new Pointer<T>(pointer._address - offset * sizeof(T));
    public static Pointer<T> operator +(Pointer<T> pointer, long offset) => new Pointer<T>(pointer._address + offset * sizeof(T));
    public static Pointer<T> operator -(Pointer<T> pointer, long offset) => new Pointer<T>(pointer._address - offset * sizeof(T));
    public static Pointer<T> operator ++(Pointer<T> pointer) => new Pointer<T>(pointer._address + sizeof(T));
    public static Pointer<T> operator --(Pointer<T> pointer) => new Pointer<T>(pointer._address - sizeof(T));
}