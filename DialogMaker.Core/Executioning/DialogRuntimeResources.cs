using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.SavedStates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Executioning
{
    public class DialogRuntimeResources : IDialogExecutionResources
    {
        public DialogRuntimeResources(IResourcesOwner resourceOwner, DialogResourcesSavedState savedState)
        {
            ResourcesOwner = resourceOwner;

            Dictionary<int, DialogItemReference> references = [];

            foreach (var info in savedState.ResourceReferences)
            {
                var value = DialogItemReference.Parse(info.Value);
                references.Add(info.Key, value);
            }

            Items = new(references);
        }
        public DialogRuntimeResources(IResourcesOwner resourcesOwner, IDictionary<int, DialogItemReference> items)
        {
            ResourcesOwner = resourcesOwner;
            Items = new(items);
        }

        public IResourcesOwner ResourcesOwner { get; }
        public ReadOnlyDictionary<int, DialogItemReference> Items { get; }

        private readonly Dictionary<int, IResourceItem> _resources = [];

        #region Управление

        public DialogResourcesSavedState Save()
        {
            DialogResourcesSavedState result = new();

            foreach (var info in Items)
            {
                result.ResourceReferences.Add(info.Key, info.Value.ToString());
            }

            return result;
        }

        public IResourceItem GetResource(int index)
        {
            if (TryGetResource(index, out var resource))
            {
                return resource;
            }

            throw GetIndexException(index);
        }
        public OperandValue GetVariable(int index)
        {
            if (!TryGetResource(index, out var resource))
            {
                throw GetIndexException(index);
            }
            if (resource is IVariable variable)
            {
                return variable.Value;
            }

            throw new ArgumentException($"Ресурс {resource} по индексу {index} не является переменной!", nameof(index));
        }
        public void SetVariable(int index, OperandValue value)
        {
            if (!TryGetResource(index, out var resource))
            {
                throw GetIndexException(index);
            }
            if (resource is IVariable variable)
            {
                if (!variable.IsReadOnly)
                {
                    variable.Value = value;
                }

                return;
            }

            throw new ArgumentException($"Ресурс {resource} по индексу {index} не является переменной!", nameof(index));
        }

        private bool TryGetResource(int index, [NotNullWhen(true)] out IResourceItem? result)
        {
            if (_resources.TryGetValue(index, out result))
            {
                return true;
            }

            var reference = Items[index];
            result = reference.GetItem(ResourcesOwner);

            _resources.Add(index, result);

            return true;
        }


        #endregion

        #region Статика

        private static ArgumentException GetIndexException(int index)
        {
            return new ArgumentException($"Не удалось найти ресурс с индексом {index}", nameof(index));
        }

        #endregion
    }
}
