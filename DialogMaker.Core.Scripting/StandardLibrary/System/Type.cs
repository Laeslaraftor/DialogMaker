namespace System;

using Internal.System.Runtime;

public class Type
{
    internal Type(RuntimeTypeInfo typeInfo)
    {
        _typeInfo = typeInfo;
    }

    public string Name
    {
        get
        {
            if (_name == null)
            {
                _name = new(_typeInfo.Name.ToSpan());
            }

            return _name;
        }
    }
    public string Namespace { get; }
    public string FullName { get; }
    public bool IsValueType { get; }

    private readonly RuntimeTypeInfo _typeInfo;
    private string? _name;

    public override string ToString() => Name;
}