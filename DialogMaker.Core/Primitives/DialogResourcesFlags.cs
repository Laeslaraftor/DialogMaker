using System;

namespace DialogMaker.Core
{
    [Flags]
    public enum DialogResourcesFlags
    {
        Root,
        Pack,
        Dialog,

        All = Root | Pack | Dialog
    }
}
