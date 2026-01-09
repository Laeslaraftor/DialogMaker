using Acly;
using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectSimpleChoiceNode : DialogProjectChoiceNode
    {
        public DialogProjectSimpleChoiceNode(DialogProjectDialog dialog)
            : base(dialog)
        {
        }
        public DialogProjectSimpleChoiceNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.SimpleChoice;
        [Name("Варианты ответа"), ItemName("Вариант ответа"), Reference(DialogResourceType.String)]
        public EditableCollection<string> Variants
        {
            get
            {
                field ??= new(() => string.Empty);
                return field;
            }
        }

        #region Управление

        protected override IStringCollection GetChoiceVariants()
        {
            List<IResourceString> strings = new(Variants.Count);

            for (int i = 0; i < Variants.Count; i++)
            {
                strings.Add(new ResourceString(i, Variants[i]));
            }

            return new LocalStringCollection(Id.ToString(), strings);
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(Variants), Variants);
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);

            var variants = savedState.GetProperty<IEnumerable<string>>(nameof(Variants));

            if (variants == null)
            {
                return;
            }

            foreach (var variant in variants)
            {
                Variants.Add(variant);
            }
        }

        #endregion
    }
}
