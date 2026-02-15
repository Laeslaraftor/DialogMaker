using DialogMaker.Core.Executioning;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace DialogMaker.Core.Common
{
    internal class DialogIndexedResources(Dialog dialog, ReadOnlyDictionary<int, DialogItemReference> indexes) 
        : IDialogExecutionResources
    {
        public Dialog Dialog { get; } = dialog;
        public ReadOnlyDictionary<int, DialogItemReference> Indexes { get; } = indexes;

        private readonly Dictionary<int, IResourceItem> _resources = [];

        #region Управление

        public IResourceItem GetItemFromReference(DialogItemReference reference)
        {
            return reference.GetItem(Dialog);
        }

        public IResourceItem GetResource(int index)
        {
            if (_resources.TryGetValue(index, out var result))
            {
                return result;
            }
            if (Indexes.TryGetValue(index, out var reference))
            {
                result = reference.GetItem(Dialog);
                _resources.Add(index, result);

                return result;
            }

            throw new ArgumentException($"Не удалось найти ресурс с индексом {index}", nameof(index));
        }
        public OperandValue GetVariable(int index)
        {
            if (GetResource(index) is IVariable variable)
            {
                return variable.Value;
            }

            return 0;
        }
        public void SetVariable(int index, OperandValue value)
        {
            if (GetResource(index) is IVariable variable)
            {
                variable.Value = value;
            }
        }

        #endregion
    }
}
