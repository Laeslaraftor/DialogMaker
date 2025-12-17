using Acly;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace DialogMaker.Core.Editor.Nodes
{
    [Name("Вариант ответа")]
    public class DialogProjectChoiceNode : DialogProjectDialogNode
    {
        public DialogProjectChoiceNode(DialogProjectDialog dialog) : base(dialog)
        {
        }
        public DialogProjectChoiceNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState) : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.Choice;
        [Name("Персонаж"), Reference(DialogResourceType.Character)]
        public DialogProjectReference<DialogProjectCharacter>? Character
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Character));
                    field = value;
                    InvokePropertyChanged(nameof(Character));
                }
            }
        }
        [Name("Варианты ответа"), ItemName("Вариант ответа"), Reference(DialogResourceType.String)]
        public EditableCollection<DialogProjectReference<DialogProjectString>> Variants { get; } = [];
        [NodeInput("Вход"), Description("Вход в узел")]
        public DialogProjectNodeInputAction Input
        {
            get
            {
                field ??= new(this, 0);
                return field;
            }
        }
        [NodeOutput("Выход"), Description("Действие после выбора ответа")]
        public DialogProjectNodeOutput Output
        {
            get
            {
                field ??= new(this, 1, DialogNodePortType.Action);
                return field;
            }
        }
        [NodeOutput("Варианты ответа"), Description("Индекс выбранного варианта ответа")]
        public DialogProjectNodeOutput SelectedVarianIndex
        {
            get
            {
                field ??= new(this, 2, DialogNodePortType.Integer);
                return field;
            }
        }

        #region Управление

        protected override DialogProjectDialogNodeSavedState CreateSavedState()
        {
            var savedState = base.CreateSavedState();
            var variants = Variants.Select(v => v.Save()).ToList();

            savedState.Properties.TryAdd(nameof(Character), Character?.Save());
            savedState.Properties.TryAdd(nameof(Variants), variants);

            return savedState;
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);

            Character = savedState.RestoreReference<DialogProjectCharacter>(Project, nameof(Character));

            var variants = savedState.GetProperty<DialogProjectReferenceSavedState[]>(nameof(Variants));

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
