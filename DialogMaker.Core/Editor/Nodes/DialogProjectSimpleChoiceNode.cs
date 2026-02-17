using Acly;
using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;
using DialogMaker.Core.Executioning.Internal;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections.Specialized;

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
        [Name("Варианты ответа"), ItemName("Вариант ответа")]
        [AllowedTypes(AllowedObjectValues.String | AllowedObjectValues.Resource), Reference(DialogResourceType.String)]
        public EditableCollection<object?> Variants
        {
            get
            {
                if (field == null)
                {
                    field = new(() => string.Empty);
                    field.CollectionChanged += OnVariantsCollectionChanged;
                }

                return field;
            }
        }

        #region Управление

        protected override IStringCollection GetChoiceVariants()
        {
            List<IResourceString> strings = new(Variants.Count);

            foreach (var variant in Variants)
            {
                if (variant == null)
                {
                    strings.Add(ResourceString.Empty);
                }
                else if (variant is DialogProjectReference reference)
                {
                    try
                    {
                        var resource = reference.Resolve();

                        if (resource is IResourceString resourceString)
                        {
                            strings.Add(resourceString);
                        }
                        else
                        {
                            strings.Add(new ResourceString(resource.Id, resource.ToString()));
                        }
                    }
                    catch (Exception error)
                    {
                        strings.Add(ResourceString.Empty);
                        Debug.WriteLine(error);
                    }
                }
                else if (variant is IResourceString resourceVariant)
                {
                    strings.Add(resourceVariant);
                }
                else
                {
                    strings.Add(new ResourceString(variant.ToString()));
                }
            }

            return new LocalStringCollection(Id.ToString(), strings);
        }

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);
            savedState.Properties.TryAdd(nameof(Variants), Variants.Select(v =>
            {
                VariantSavedState savedState = new();

                if (v is IResourceString resource)
                {
                    savedState.IsReference = true;
                    savedState.Value = resource.CreateReference();
                }
                else if (v is DialogProjectReference reference)
                {
                    savedState.IsReference = true;

                    try
                    {
                        savedState.Value = reference.Resolve().CreateReference();
                    }
                    catch (Exception error)
                    {
                        savedState.Value = ResourceString.Empty;
                        Debug.WriteLine(error);
                    }
                }
                else
                {
                    savedState.Value = v?.ToString();
                }

                return savedState;
            }));
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            base.Restore(savedState);

            var variants = savedState.GetProperty<IEnumerable<VariantSavedState>>(nameof(Variants));

            if (variants == null)
            {
                return;
            }

            foreach (var variant in variants)
            {
                if (variant.Value == null)
                {
                    Variants.Add(ResourceString.Empty);
                    continue;
                }
                if (variant.IsReference && variant.Value is JToken token)
                {
                    var reference = token.ToObject<DialogItemReference>().GetItem(Owner);

                    if (reference is DialogProjectResourceObject resourceObject)
                    {
                        Variants.Add(DialogProjectReference.Create(resourceObject));
                        continue;
                    }

                    Variants.Add(reference);
                }
                else
                {
                    Variants.Add(variant.Value?.ToString());
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Variants.CollectionChanged -= OnVariantsCollectionChanged;
        }

        #endregion

        #region События

        private void OnVariantsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ExtraOutputs.Clear();
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    DialogProjectNodeOutputAction output = new(this, GetNextExtraPortId());
                    ExtraOutputs.Add(output, new($"Вариант {e.NewStartingIndex}", string.Empty));
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && ExtraOutputs.Count > 0)
            {
                var last = ExtraOutputs.Last().Key;

                if (ExtraOutputs.Remove(last))
                {
                    last.Dispose();
                }
            }
        }

        #endregion

        #region Классы

        private struct VariantSavedState
        {
            public bool IsReference { get; set; }
            public object? Value { get; set; }
        }

        #endregion
    }
}
