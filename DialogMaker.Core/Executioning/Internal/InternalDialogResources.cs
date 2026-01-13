using DialogMaker.Core.Common;
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
        protected readonly Dictionary<int, OperandValue> Variables = [];
        protected readonly List<int> UsedGlobalVariables = [];

        #region Управление

        public IResourceItem GetResource(int index)
        {
            if (Items.TryGetValue(index, out var item))
            {
                return item;
            }
            if (Metadata.LocalValues.TryGetValue(index, out var localReference))
            {
#pragma warning disable CS8625 // Литерал, равный NULL, не может быть преобразован в ссылочный тип, не допускающий значение NULL.
                item = localReference.GetItem(null);
#pragma warning restore CS8625 // Литерал, равный NULL, не может быть преобразован в ссылочный тип, не допускающий значение NULL.
                Items.Add(index, item);
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
            if (Variables.TryGetValue(index, out var value))
            {
                return value;
            }
            if (Metadata.LocalValues.TryGetValue(index, out var localReference))
            {
#pragma warning disable CS8625 // Литерал, равный NULL, не может быть преобразован в ссылочный тип, не допускающий значение NULL.
                var item = localReference.GetItem(null);
#pragma warning restore CS8625 // Литерал, равный NULL, не может быть преобразован в ссылочный тип, не допускающий значение NULL.

                if (item is not IVariable variable)
                {
                    throw new InvalidCastException($"Получен неожиданный ресурс из локальной ссылки на ресурс: {item}");
                }

                Variables.Add(index, variable.Value);

                return variable.Value;
            }

            value = Resources.GetVariable(index);
            UsedGlobalVariables.Add(index);
            Variables.Add(index, value);

            return value;
        }
        public virtual void SetVariable(int index, OperandValue value)
        {
            SetVariable(index, value, false);
        }

        protected void SetVariable(int index, OperandValue value, bool onlyLocal)
        {
            if (Metadata.LocalValues.ContainsKey(index) || onlyLocal)
            {
                if (!Variables.TryAdd(index, value))
                {
                    Variables[index] = value;
                }

                return;
            }

            Resources.SetVariable(index, value);
        }

        #endregion
    }
}
