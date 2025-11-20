using System;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectDialogSavedState : JsonData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DialogProjectDialogNodeSavedState[] Nodes { get; set; } = Array.Empty<DialogProjectDialogNodeSavedState>();
        public DialogProjectCharacterSavedState[] Characters { get; set; } = Array.Empty<DialogProjectCharacterSavedState>();
    }
}
