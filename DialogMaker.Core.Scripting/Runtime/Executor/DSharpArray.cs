using System.Collections;
using System.Runtime.InteropServices;

namespace DialogMaker.Core.Scripting.Runtime.Executor
{
    /// <summary>
    /// D# array
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DSharpArray
    {
        /// <summary>
        /// Current D# object
        /// </summary>
        public DSharpObject Object;
        /// <summary>
        /// Size for managed data, this not includes size of DSharpObject structure size.
        /// </summary>
        public int Size;
        /// <summary>
        /// Length if type is array
        /// </summary>
        public int Length;

        public readonly override string ToString()
        {
            return $"{Object}: {Length}";
        }

        #region Satatic

        /// <summary>
        /// Get D# array length
        /// </summary>
        /// <param name="obj">D# array instance</param>
        /// <returns>Array length</returns>
        public static int GetLength(DSharpObject* obj)
        {
            if (obj->IsArray)
            {
                return ((DSharpArray*)obj)->Length;
            }

            return 0;
        }

        #endregion
    }
}
