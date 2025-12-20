using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;

namespace DialogMaker.Editor.Nodes
{
    public class ProjectNodesCopiedState : Disposable
    {
        public ProjectNodesCopiedState(IEnumerable<DialogProjectDialogNodeSavedState> savedStates)
        {
            _savedStates = savedStates;
        }

        private readonly Dictionary<Guid, Guid> _identifiersMap = [];
        private readonly IEnumerable<DialogProjectDialogNodeSavedState> _savedStates;

        #region Управление

        public IEnumerable<DialogProjectDialogNode> Paste(DialogProjectDialog dialog)
        {
            UpdateIdentifiersMap();
            return dialog.RestoreNode(_savedStates, e =>
            {
                e.Alert();
            });
        }

        private void UpdateIdentifiersMap()
        {
            _identifiersMap.Clear();

            foreach (var savedState in _savedStates)
            {
                _identifiersMap.Add(savedState.Id, Guid.NewGuid());
            }
            foreach (var savedState in _savedStates)
            {
                savedState.ReplaceIds(_identifiersMap);
            }
        }

        #endregion
    }
}
