using System.Collections;
#if NETSTANDARD2_1_OR_GREATER
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#endif

namespace DialogMaker.Core.Scripting
{
    internal static class InternalExtensions
    {
#if NETSTANDARD2_1_OR_GREATER
        extension(ObjectDisposedException)
        {
            [StackTraceHidden, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void ThrowIf([DoesNotReturnIf(true)] bool condition, object instance)
            {
                if (condition)
                {
                    throw new ObjectDisposedException(instance.GetType().Name);
                }
            }
        }
#endif

        extension(IEnumerable enumerable)
        {
            public int IndexOf(object item)
            {
                int index = 0;

                foreach (var value in enumerable)
                {
                    if (Equals(value, item))
                    {
                        return index;
                    }

                    index++;
                }

                return -1;
            }
        }
    }
}
