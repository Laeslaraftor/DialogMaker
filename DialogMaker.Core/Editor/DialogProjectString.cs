using Acly;
using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
using System;
using System.ComponentModel;
using DialogMaker.Core.Editor;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectString : DialogProjectResourceObject, IResourceString
    {
        public DialogProjectString(DialogProjectResources resources)
            : this(resources, Guid.NewGuid())
        {
        }
        public DialogProjectString(DialogProjectResources resources, DialogProjectStringSavedState savedState)
            : this(resources, Guid.Parse(savedState.ProjectId), savedState.Id)
        {
            if (savedState.Variants != null)
            {
                foreach (var variantSavedState in savedState.Variants)
                {
                    try
                    {
                        Variants.Add(new(this, variantSavedState));
                    }
                    catch (Exception error)
                    {
                        Logger.Log(error);
                    } 
                }
            }
        }
        private DialogProjectString(DialogProjectResources resources, Guid projectId, string? id = null)
            : base(resources, projectId)
        {
            Variants = new();

            id = id?.Trim();

            if (!string.IsNullOrEmpty(id))
            {
                Id = id;
            }

            Variants.ItemChanged += OnVariantsItemChanged;
        }

        public override DialogResourceType ResourceType => DialogResourceType.String;
        public string Preview
        {
            get
            {
                if (Variants.Count == 0)
                {
                    return string.Empty;
                }
                if (Resources.Owner.Project.DefaultLanguage != null &&
                    TryGetVariant(Resources.Owner.Project.DefaultLanguage, out var variant))
                {
                    return variant.Text;
                }

                return Variants[0].Text;
            }
        }
        public EditableCollection<DialogProjectStringVariant> Variants { get; }

        string IResourceString.Text => Preview;
        IResourceFile? IResourceString.Voice
        {
            get
            {
                if (Variants.Count == 0)
                {
                    return null;
                }
                if (Resources.Owner.Project.DefaultLanguage != null &&
                    TryGetVariant(Resources.Owner.Project.DefaultLanguage, out var variant))
                {
                    return variant.Voice?.Resolve();
                }

                return Variants[0].Voice?.Resolve();
            }
        }

        #region Управление

        public override IVariable ToVariable()
        {
            return new LocalVariable(Id, Preview);
        }

        public bool TryGetVariant(DialogProjectLanguage language, [NotNullWhen(true)] out DialogProjectStringVariant? result)
        {
            return Variants.TryGetValue(l => l.Language == language, out result);
        }

        public DialogProjectStringVariant CreateVariant()
        {
            DialogProjectStringVariant result = new(this);
            Variants.Add(result);

            return result;
        }
        public bool Remove(DialogProjectStringVariant variant)
        {
            return Variants.Remove(variant);
        }

        protected override DialogProjectResourceObjectSavedState CreateSavedState()
        {
            return new DialogProjectStringSavedState
            {
                Variants = Variants.Select(v => (DialogProjectStringVariantSavedState)v.Save()).ToArray()
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Variants.Clear();
            Variants.ItemChanged -= OnVariantsItemChanged;
        }

        public override string ToString()
        {
            return Preview.Trim();
        }

        #endregion

        #region События

        private void OnVariantsItemChanged(object sender, CollectionItemEventArgs<DialogProjectStringVariant> e)
        {
            if (e.Action == CollectionItemAction.Add)
            {
                e.Item.PropertyChanged -= OnVariantPropertyChanged;
                e.Item.PropertyChanged += OnVariantPropertyChanged;
            }
            else if (e.Action == CollectionItemAction.Remove)
            {
                e.Item.PropertyChanged -= OnVariantPropertyChanged;
            }
        }

        private void OnVariantPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Text")
            {
                InvokePropertyChanged(nameof(Preview));
                return;
            }
            if (sender is not DialogProjectStringVariant variant || 
                e.PropertyName != "Language" || 
                variant.Language == null)
            {
                return;
            }

            foreach (var otherVariant in Variants)
            {
                if (otherVariant != variant && 
                    otherVariant.Language == variant.Language)
                {
                    throw new ArgumentException("Невозможно задать язык, так как вариант для этого языка уже существует", "Language");
                }
            }
        }

        #endregion
    }
}
