#if NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
#endif

namespace DialogMaker.Core.Scripting
{
    internal static class InternalExtensions
    {
#if NETSTANDARD2_1_OR_GREATER
        extension(ObjectDisposedException)
        {
            [StackTraceHidden]
            public static void ThrowIf([DoesNotReturnIf(true)] bool condition, object instance)
            {
                if (condition)
                {
                    throw new ObjectDisposedException(instance.GetType().Name);
                }
            }
        }
#endif
    }
}
