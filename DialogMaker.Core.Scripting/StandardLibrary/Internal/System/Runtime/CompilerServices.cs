using System.Native;

namespace Internal.System.Runtime;

internal static class CompilerServices
{
    public static extern nint GetObjectAddress(object obj);
    public static nint GetObjectTypeToken(object obj)
    {
        var address = GetObjectAddress(obj);

        if (address == 0)
        {
            return 0;
        }

        return *(nint*)address;
    }
}