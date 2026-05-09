using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor;

namespace DialogMaker.Lib.Data
{
    public class TriggerPresetNodeSelectorItemInfo : NodeSelectorItemInfo
    {
        public DialogProjectTriggerPreset? TriggerPreset
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(TriggerPreset));
                    field = value;
                    OnPropertyChanged(nameof(TriggerPreset));
                }
            }
        }

        #region Управление

        protected override DialogProjectDialogNode CreateNode(ProjectDialog dialog)
        {
            var node = base.CreateNode(dialog);
            var preset = TriggerPreset;

            if (node is DialogProjectTriggerNode triggerNode &&
                preset != null)
            {
                preset.SetupNode(triggerNode);
            }

            return node;
        }

        #endregion
    }
}
