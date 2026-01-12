using DialogMaker.Core.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DialogMaker.Core.Executioning.Internal
{
    public readonly struct JoinOperationInfo(string id, IList<int> inputs, IList<DialogPosition> outputs) : IJoinOperationInfo
    {
        public JoinOperationInfo(IList<int> inputs, IList<DialogPosition> outputs)
            : this(Guid.NewGuid().ToString(), inputs, outputs)
        {

        }

        public string Id { get; } = id;
        public DialogResourceType ResourceType => DialogResourceType.String;
        public bool IsSeparated => true;
        public ReadOnlyCollection<DialogPosition> Outputs { get; } = new(outputs);
        public ReadOnlyCollection<int> InputSections { get; } = new(inputs);

        #region Управление

        public ResourcePath GetPath()
        {
            throw new InvalidOperationException(IResourceItem.GetPathExceptionMessage);
        }
        public DialogItemReference CreateReference()
        {
            return DialogItemReference.Create(this);
        }
        public IVariable ToVariable()
        {
            return new LocalVariable(0);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Outputs, InputSections);
        }
        public override string ToString()
        {
            string inputs = string.Empty;
            string outputs = string.Empty;

            foreach (var section in InputSections)
            {
                if (inputs != string.Empty)
                {
                    inputs += ", ";
                }

                inputs += section;
            }
            foreach (var output in Outputs)
            {
                if (outputs != string.Empty)
                {
                    outputs += ", ";
                }

                outputs += output.ToString();
            }

            return $"[{Id}] {inputs} -> {outputs}";
        }

        #endregion
    }
}
