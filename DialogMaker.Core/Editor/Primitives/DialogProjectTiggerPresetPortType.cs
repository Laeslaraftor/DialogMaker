using System;
using System.Collections.Generic;
using System.Text;

namespace DialogMaker.Core.Editor.Primitives
{
    public enum DialogProjectTiggerPresetPortType : byte
    {
        Number = DialogNodePortType.Number,
        String = DialogNodePortType.String,
        Bool = DialogNodePortType.Bool,
        Object = DialogNodePortType.Object
    }
}
