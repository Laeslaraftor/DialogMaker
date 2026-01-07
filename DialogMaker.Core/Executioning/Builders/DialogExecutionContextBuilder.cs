using Acly;
using DialogMaker.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DialogMaker.Core.Executioning.Builders
{
    public class DialogExecutionContextBuilder : IDialogExecutionResources
    {
        public DialogExecutionContextBuilder()
        {
            Resources = new(_resources);
            Variables = new(_variables);
        }

        public ReferenceReadOnlyDictionary<int, IResourceItem> Resources { get; }
        public ReferenceReadOnlyDictionary<int, IVariable> Variables { get; }

        private readonly ObservableDictionary<int, IResourceItem> _resources = [];
        private readonly ObservableDictionary<int, IVariable> _variables = [];

        #region Управление

        public IDictionary<int, DialogItemReference> Build()
        {
            SortedDictionary<int, DialogItemReference> result = [];

            foreach (var info in _resources)
            {
                result.Add(info.Key, info.Value.CreateReference());
            }
            foreach (var info in _variables)
            {
                result.Add(info.Key, info.Value.CreateReference());
            }

            return result;
        }
        public DialogRuntimeResources Build(IResourcesOwner resourcesOwner)
        {
            var references = Build();
            return new(resourcesOwner, references);
        }

        public int GetNextIndex()
        {
            return FindFreeIndex(_resources.Keys, _variables.Keys);
        }

        public int AddResource(IResourceItem resource)
        {
            if (resource is IVariable variable)
            {
                return AddVariable(variable);
            }
            if (TryFindItemIndex(_resources, resource, out var index))
            {
                return index;
            }

            index = GetNextIndex();
            _resources.Add(index, resource);

            return index;
        }
        public int AddVariable(IVariable variable)
        {
            if (TryFindItemIndex(_variables, variable, out var index))
            {
                return index;
            }

            index = GetNextIndex();
            _variables.Add(index, variable);

            return index;
        }

        public void SetResource(int index, IResourceItem resource)
        {
            _resources[index] = resource;
        }
        public void SetVariable(int index, IVariable variable)
        {
            _variables[index] = variable;
        }

        public bool Remove(int index)
        {
            if (_resources.Remove(index) ||
                _variables.Remove(index))
            {
                return true;
            }

            return false;
        }

        public IResourceItem GetResource(int index)
        {
            if (_resources.TryGetValue(index, out var resource))
            {
                return resource;
            }
            if (_variables.TryGetValue(index, out var variable))
            {
                return variable;
            }

            throw new ArgumentException($"Ресурс с индексом {index} не найден");
        }
        public OperandValue GetVariable(int index)
        {
            if (_variables.TryGetValue(index, out var variable))
            {
                return variable.Value;
            }
            if (_resources.TryGetValue(index, out var resource) &&
                resource is IVariable resourceVariable)
            {
                return resourceVariable.Value;
            }

            throw new ArgumentException($"Переменная с индексом {index} не найден");
        }
        public void SetVariable(int index, OperandValue value)
        {
            if (_variables.TryGetValue(index, out var variable))
            {
                variable.Value = value;
            }
            if (_resources.TryGetValue(index, out var resource) &&
                resource is IVariable resourceVariable)
            {
                resourceVariable.Value = value;
            }
        }

        #endregion

        #region Статика

        private static int FindFreeIndex(ICollection<int> indexes)
        {
            int index = 0;

            foreach (var i in indexes)
            {
                if (i > index)
                {
                    return index;
                }

                index++;
            }

            return index;
        }
        private static int FindFreeIndex(params ICollection<int>[] indexes)
        {
            int max = -1;

            foreach (var indexesCollection in indexes)
            {
                foreach (var value in indexesCollection)
                {
                    max = Math.Max(max, value);
                }
            }

            return max + 1;
        }
        private static bool TryFindItemIndex<T>(IDictionary<int, T> items, T item, [NotNullWhen(true)] out int index)
            where T : notnull
        {
            index = -1;

            foreach (var info in items)
            {
                if (item.Equals(info.Value))
                {
                    index = info.Key;
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
