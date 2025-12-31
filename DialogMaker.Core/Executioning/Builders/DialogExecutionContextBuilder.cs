using Acly;
using DialogMaker.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Executioning.Builders
{
    public class DialogExecutionContextBuilder : IDialogExecutionContext
    {
        public DialogExecutionContextBuilder()
        {
            Resources = new(_resources);
            Strings = new(_strings);
            Variables = new(_variables);
        }

        public ReferenceReadOnlyDictionary<int, IResourceItem> Resources { get; }
        public ReferenceReadOnlyDictionary<int, IResourceString> Strings { get; }
        public ReferenceReadOnlyDictionary<int, IVariable> Variables { get; }

        private readonly ObservableDictionary<int, IResourceItem> _resources = [];
        private readonly ObservableDictionary<int, IResourceString> _strings = [];
        private readonly ObservableDictionary<int, IVariable> _variables = [];

        #region Управление

        public int GetNextIndex()
        {
            return FindFreeIndex(_resources.Keys, _strings.Keys, _variables.Keys);
        }

        public int AddResource(IResourceItem resource)
        {
            if (TryFindItemIndex(_resources, resource, out var index))
            {
                return index;
            }

            index = GetNextIndex();
            _resources.Add(index, resource);

            return index;
        }
        public int AddString(IResourceString text)
        {
            if (TryFindItemIndex(_strings, text, out var index))
            {
                return index;
            }

            index = GetNextIndex();
            _strings.Add(index, text);

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
        public void SetString(int index, IResourceString text)
        {
            _strings[index] = text;
        }
        public void SetVariable(int index, IVariable variable)
        {
            _variables[index] = variable;
        }

        public bool Remove(int index)
        {
            if (_resources.Remove(index) ||
                _strings.Remove(index) || 
                _variables.Remove(index))
            {
                return true;
            }

            return false;
        }

        public IResourceItem GetResource(int index)
        {
            return _resources[index];
        }
        public string GetString(int index)
        {
            return _strings[index].Text;
        }
        public OperandValue GetVariable(int index)
        {
            return _variables[index].Value;
        }
        public void SetVariable(int index, OperandValue value)
        {
            _variables[index].Value = value;
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
            int max = 0;

            foreach (var indexesCollection in indexes)
            {
                max = Math.Max(max, FindFreeIndex(indexesCollection));
            }

            return max;
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
