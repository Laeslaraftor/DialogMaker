using DialogMaker.Core.Attributes;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Core
{
    public enum DialogResourceType
    {
        [ResourceType(typeof(DialogProjectString), IsDev = true)]
        String,
        [ResourceType(typeof(DialogProjectCharacter), IsDev = true)]
        Character,
        [ResourceType(typeof(DialogProjectItem), IsDev = true)]
        File,
        [ResourceType(typeof(DialogProjectVariable), IsDev = true)]
        Variable,
    }
}
