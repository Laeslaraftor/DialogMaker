namespace System.Reflection;

public class PropertyInfo : MemberInfo
{
    public override string Name => throw new NotImplementedException();
    public override Type? DeclaringType => throw new NotImplementedException();
    public Type PropertyType { get; }
    public bool CanRead => GetterMethod != null;
    public bool CanWrite => SetterMethod != null;
    public MethodInfo? GetterMethod { get; }
    public MethodInfo? SetterMethod { get; }

    public object? Read(object? instance)
    {
        if (GetterMethod != null)
        {
            return GetterMethod.Invoke(instance, null);
        }

        throw new InvalidOperationException("Unable to read property without getter");
    }
    public object? Write(object? instance, object? value)
    {
        if (SetterMethod != null)
        {
            return SetterMethod.Invoke(instance, new object[] { value });
        }

        throw new InvalidOperationException("Unable to write to property without setter");
    }
}