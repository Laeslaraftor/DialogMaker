using DialogMaker.Core.Editor.Messages;

namespace DialogMaker.Lib
{
    public enum MessageType
    {
        Normal = MessageImportance.Normal,
        Warning = MessageImportance.Warning,
        Error = MessageImportance.Critical,
        Success,
    }
}
