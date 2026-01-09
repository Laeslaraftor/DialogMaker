using Acly;
using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectResourceChoiceNode : DialogProjectChoiceNode
    {
        public DialogProjectResourceChoiceNode(DialogProjectDialog dialog) 
            : base(dialog)
        {
        }
        public DialogProjectResourceChoiceNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) 
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Choice;
        [Name("Варианты ответа"), ItemName("Вариант ответа"), Reference(DialogResourceType.String)]
        public EditableCollection<DialogProjectReference<DialogProjectString>> Variants
        {
            get
            {
                field ??= new(() => new(Dialog.Project));
                return field;
            }
        }

        #region Управление

        protected override IStringCollection GetChoiceVariants()
        {
            return new LocalStringCollection(Id.ToString(), [.. Variants.Select(v => v.Resolve())]);
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);

            var variants = Variants.Select(v => v.Save()).ToList();
            savedState.Properties.TryAdd(nameof(Variants), variants);
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);

            var variants = savedState.GetProperty<IEnumerable<DialogProjectReferenceSavedState>>(nameof(Variants));

            if (variants == null)
            {
                return;
            }

            foreach (var variant in variants)
            {
                DialogProjectReference<DialogProjectString> reference;

                try
                {
                    reference = DialogProjectReference<DialogProjectString>.Restore(Project, variant);
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                    continue;
                }

                Variants.Add(reference);
            }
        }

        #endregion
    }
}
