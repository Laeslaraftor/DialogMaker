using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Editor.Nodes
{
    public interface INode : INotifyPropertyChanged, IDisposable
    {
        public bool IsDisposed { get; }
        public Guid Id { get; }
        public DialogNodeType NodeType { get; }
        public IPortDataConverter DataConverter { get; }
        public bool IsImmediate { get; }
        public bool IsCodeGenerator { get; }
        public bool IsSystem { get; }
        public bool IsSeparator { get; }
        public bool IsFunction { get; }
        public bool IsUserHandleNode { get; }
        public bool CanBeEntryPoint { get; }

        public IEnumerable<INode> GetLocalGroup(Predicate<INode> ignorePredicate);
        public void Compile(DialogCompilerContext context);
        public bool TryGetResourceValue(DialogProjectNodeOutput port, [NotNullWhen(true)] out IResourceItem? resource);
        public IEnumerable<DialogProjectNodePort> GetPorts();
        public ReadOnlyDictionary<DialogProjectNodeInput, DialogProjectNodeMetadata> GetInputs();
        public ReadOnlyDictionary<DialogProjectNodeOutput, DialogProjectNodeMetadata> GetOutputs();
        public string? GetName(DialogProjectNodePort port);
    }
}
