namespace System;

using Internal.System.Runtime;

public class Type : IEquatable<Type>
{
    internal Type(RuntimeTypeInfo typeInfo)
    {
        _typeInfo = typeInfo;
    }

    public string Name
    {
        get
        {
            field ??= new(_typeInfo.Name.ToSpan());
            return field;
        }
    }
    public string Namespace 
    {
        get
        {
            field ??= _typeInfo.Namespace.Length == 0 ? string.Empty : new(_typeInfo.Namespace.ToSpan());
            return field;
        }
    }
    public string FullName
    {
        get
        {
            if (field == null)
            {
                var name = Name;
                var @namespace = Namespace;
                var declaringType = DeclaringType;

                if (declaringType != null)
                {
                    field = declaringType.FullName + "." + name;
                }
                else 
                {
                    if (string.IsNullOrEmpty(@namespace))
                    {
                        field = name;
                    }
                    else
                    {
                        field = @namespace + "." + name;
                    }
                }
            }

            return field;
        }
    }
    public bool IsValueType
    {
        get
        {
            var objectType = _typeInfo.ObjectType;

            return objectType == ObjectType.Enum ||
                   objectType == ObjectType.Struct;
        }
    }
    public Type? DeclaringType
    {
        get
        {
            if (field == null && !_typeInfo.DeclaringType.IsNull)
            {
                field = new(_typeInfo.DeclaringType[0]);
            }

            return field;
        }
    }

    private readonly RuntimeTypeInfo _typeInfo;

    public Type[] GetInterfaces()
    {
        Type[] result = new Type[_typeInfo.Intefaces.Length];

        for (int i = 0; i < _typeInfo.Intefaces.Length; i++)
        {
            result = new Type(_typeInfo.Intefaces[i][0]);
        }

        return result;
    }

    public override string ToString() => FullName;
    public override bool Equals(object? obj)
    {
        return obj is Type other && Equals(other);
    }
    public bool Equals(Type other)
    {
        if (other == null)
        {
            return false;
        }

        return _typeInfo.MetadataToken == other._typeInfo.MetadataToken;
    }
}