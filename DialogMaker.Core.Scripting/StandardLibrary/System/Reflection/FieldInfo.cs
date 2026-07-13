namespace System.Reflection;

public class FieldInfo : MemberInfo
{
    public override string Name => throw new NotImplementedException();
    public override Type? DeclaringType => throw new NotImplementedException();
    public Type FieldType { get; }
    public bool IsReadOnly { get; }

    public object? Read(object? instance)
    {
        throw new NotImplementedException();
    }
    public object? Write(object? instance, object? value)
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException("Unable to write to read-only field");
        }

        throw new NotImplementedException();
    }
}