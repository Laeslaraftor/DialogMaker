namespace Internal.System.Runtime;

internal enum MethodType : byte
{
    Default,
    Getter,
    Setter,
    Constructor,
    Finalizer,
    Initializer,
    Operator
}