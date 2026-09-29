namespace Internal.System.Runtime;

internal enum DSharpMethodParameterMode : byte
{   Default,
    This,
    Ref,
    Out,
    Params,
}