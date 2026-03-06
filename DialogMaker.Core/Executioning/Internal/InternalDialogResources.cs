using DialogMaker.Core.Common;
using DialogMaker.Core.Editor;
using System;
using System.Collections.Generic;

namespace DialogMaker.Core.Executioning.Internal
{
    internal class InternalDialogResources(DialogByteCodeData data, IDialogExecutionResources resources)
        : IDialogExecutionResources
    {
        public DialogByteCodeData Data { get; } = data;
        public DialogMetadata Metadata { get; } = data.Metadata;
        public IDialogExecutionResources Resources { get; } = resources;

        protected readonly Dictionary<int, IResourceItem> Items = [];
        protected readonly List<int> UsedGlobalVariables = [];

        #region Управление

        public void Reset()
        {
            Resources.Reset();
            Items.Clear();
            UsedGlobalVariables.Clear();
        }

        public IResourceItem GetItemFromReference(DialogItemReference reference)
        {
            return Resources.GetItemFromReference(reference);
        }

        public IResourceItem GetResource(int index)
        {
            if (Items.TryGetValue(index, out var item))
            {
                return item;
            }
            if (Metadata.LocalValues.TryGetValue(index, out var localReference))
            {
                try
                {
                    item = GetItemFromReference(localReference);
                    Items.Add(index, item);
                }
                catch (Exception error)
                {
                    Logger.Log(error);
                }
            }
            if (item == null)
            {
                item = Resources.GetResource(index);
                Items.Add(index, item);
            }

            return item;
        }
        public OperandValue GetVariable(int index)
        {
            if (Items.TryGetValue(index, out var resource))
            {
                return DialogRuntimeResources.ToOperandValue(resource);
            }
            if (Metadata.LocalValues.TryGetValue(index, out var localReference))
            {
                var item = GetItemFromReference(localReference);

                if (item is not IVariable variable)
                {
                    throw new InvalidCastException($"Получен неожиданный ресурс из локальной ссылки на ресурс: {item}");
                }

                Items.Add(index, new LocalVariable(variable.Value));

                return variable.Value;
            }

            var value = Resources.GetVariable(index);
            UsedGlobalVariables.Add(index);
            Items.Add(index, new LocalVariable(value));

            return value;
        }
        public virtual void SetValue(int index, OperandValue value)
        {
            SetVariable(index, value, false);
        }
        public virtual void SetValue(int index, IResourceItem resource)
        {
            SetVariable(index, resource, false);
        }

        protected void SetVariable(int index, object value, bool onlyLocal)
        {
            if (Metadata.LocalValues.ContainsKey(index) || onlyLocal)
            {
                if (value is OperandValue operandValue)
                {
                    SetVariable(index, operandValue);
                }
                else if (value is IResourceItem item)
                {
                    SetVariable(index, item);
                }

                return;
            }

            if (value is IResourceItem resource)
            {
                Resources.SetValue(index, resource);
            }
            else if (value is OperandValue operandValue)
            {
                Resources.SetValue(index, operandValue);
            }
            else
            {
                throw new ArgumentException($"Недопустимый тип значения: {value?.GetType()}");
            }
        }
        protected void SetVariable(int index, OperandValue value)
        {
            if (Items.TryGetValue(index, out var resource) &&
                resource is IVariable variable)
            {
                variable.Value = value;
            }

            LocalVariable newVariable = new(value);

            if (!Items.TryAdd(index, newVariable))
            {
                Items[index] = newVariable;
            }
        }
        protected void SetVariable(int index, IResourceItem value)
        {
            if (value is IVariable variable)
            {
                SetVariable(index, variable.Value);
                return;
            }
            if (!Items.TryAdd(index, value))
            {
                Items[index] = value;
            }
        }

        #endregion
    }
}
