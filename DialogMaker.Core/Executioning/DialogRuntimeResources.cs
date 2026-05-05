using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning.Internal;
using DialogMaker.Core.Executioning.SavedStates;
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
            : this(resourcesOwner, new ReadOnlyDictionary<int, DialogItemReference>(items))
        {
        }
        public DialogRuntimeResources(IResourcesOwner resourcesOwner, ReadOnlyDictionary<int, DialogItemReference> items)
        {
            ResourcesOwner = resourcesOwner;
            Items = items;
        }

        public IResourcesOwner ResourcesOwner { get; }
        public ReadOnlyDictionary<int, DialogItemReference> Items { get; }

        private readonly Dictionary<int, IResourceItem> _resources = [];

        #region Управление

        public void Reset()
        {
            _resources.Clear();

            foreach (var info in Items)
            {
                var item = info.Value.GetItem(ResourcesOwner);
                _resources.Add(info.Key, item);
            }
        }
        public DialogResourcesSavedState Save()
        {
            DialogResourcesSavedState result = new();

            foreach (var info in Items)
            {
                result.ResourceReferences.Add(info.Key, info.Value.ToString());
            }

            return result;
        }
        public IResourceItem GetItemFromReference(DialogItemReference reference)
        {
            return reference.GetItem(ResourcesOwner);
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
                return 0;
            }

            return ToOperandValue(resource);
        }
        public void SetValue(int index, OperandValue value)
        {
            if (!TryGetResource(index, out var resource))
            {
                _resources.Add(index, new LocalVariable(value));
                return;
            }
            if (resource is IVariable variable)
            {
                if (!variable.IsReadOnly)
                {
                    variable.Value = value;
                }

                return;
            }

            _resources[index] = new LocalVariable(value);
        }
        public void SetValue(int index, IResourceItem resource)
        {
            if (!TryGetResource(index, out var currentResource))
            {
                _resources.Add(index, resource);
                return;
            }
            if (currentResource is IVariable currentVariable &&
                resource is IVariable variable)
            {
                if (!currentVariable.IsReadOnly)
                {
                    currentVariable.Value = variable.Value;
                    return;
                }
            }

            _resources[index] = resource;
        }

        private bool TryGetResource(int index, [NotNullWhen(true)] out IResourceItem? result)
        {
            if (_resources.TryGetValue(index, out result))
            {
                return true;
            }
            if (!Items.TryGetValue(index, out var reference))
            {
                return false;
            }

            result = reference.GetItem(ResourcesOwner);

            _resources.Add(index, result);

            return true;
        }


        #endregion

        #region Статика

        internal static OperandValue ToOperandValue(IResourceItem resource, bool throwError = true)
        {
            if (resource is IVariable variable)
            {
                return variable.Value;
            }
            else if (resource is IResourceString stringResource)
            {
                return stringResource.Text;
            }
            else if (resource is ICharacter character)
            {
                return character.Name;
            }

            if (throwError)
            {
                throw new ArgumentException($"Ресурс {resource} невозможно преобразовать в значение переменной!", nameof(resource));
            }

            return 0;
        }

        private static ArgumentException GetIndexException(int index)
        {
            return new ArgumentException($"Не удалось найти ресурс с индексом {index}", nameof(index));
        }

        #endregion
    }
}
