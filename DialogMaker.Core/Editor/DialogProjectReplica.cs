using Acly;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DialogMaker.Core.Editor
{
    public class DialogProjectReplica : ObservableObject, ISavable, IDisposable
    {
        public DialogProjectReplica(DialogProjectResources resources)
            : this(resources, Guid.NewGuid())
        {
        }
        public DialogProjectReplica(DialogProjectResources resources, DialogProjectReplicaSavedState savedState)
            : this(resources, Guid.Parse(savedState.ProjectId))
        {
            Resources = resources;
            ProjectId = Guid.Parse(savedState.ProjectId);
            _id = savedState.Id;
            
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
                        Debug.WriteLine(error);
                    } 
                }
            }
        }
        private DialogProjectReplica(DialogProjectResources resources, Guid projectId)
        {
            Resources = resources;
            ProjectId = projectId;
            Variants = new();
            Variants.ItemChanged += OnVariantsItemChanged;
        }
        ~DialogProjectReplica()
        {
            Dispose();
        }

        public Guid ProjectId { get; }
        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    InvokePropertyChanged(nameof(Id));
                }
            }
        }
        public DialogProjectResources Resources { get; }
        public EditableCollection<DialogProjectReplicaVariant> Variants { get; }

        private string _id = string.Empty;

        #region Управление

        public bool TryGetVariant(DialogProjectLanguage language, [NotNullWhen(true)] out DialogProjectReplicaVariant? result)
        {
            return Variants.TryGetValue(l => l.Language == language, out result);
        }

        public DialogProjectReplicaVariant CreateVariant()
        {
            DialogProjectReplicaVariant result = new(this);
            Variants.Add(result);

            return result;
        }
        public bool Remove(DialogProjectReplicaVariant variant)
        {
            return Variants.Remove(variant);
        }

        public ISavedState Save()
        {
            return new DialogProjectReplicaSavedState
            {
                ProjectId = ProjectId.ToString(),
                Id = Id?.ToString() ?? string.Empty,
                Variants = Variants.Select(v => (DialogProjectReplicaVariantSavedState)v.Save()).ToArray()
            };
        }

        public void Dispose()
        {
            Variants.ItemChanged -= OnVariantsItemChanged;
            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            string result = $"[{Id}]";

            if (Variants.Count == 0)
            {
                return result;
            }
            if (Resources.Owner.Project.DefaultLanguage != null &&
                TryGetVariant(Resources.Owner.Project.DefaultLanguage, out var variant))
            {
                return result + $" {variant.Text}";
            }

            return result + $" {Variants[0].Text}";
        }

        #endregion

        #region События

        private void OnVariantsItemChanged(object sender, CollectionItemEventArgs<DialogProjectReplicaVariant> e)
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
            if (sender is not DialogProjectReplicaVariant variant || 
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
