using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Editor.Nodes
{
    public interface INode : INotifyPropertyChanged, IDisposable
    {
        public event EventHandler? InputsUpdated;
        public event EventHandler? OutputsUpdated;

        public IResourcesOwner Owner { get; }
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
        public ReferenceReadOnlyDictionary<DialogProjectNodeInput, DialogProjectNodeMetadata> GetInputs();
        public ReferenceReadOnlyDictionary<DialogProjectNodeOutput, DialogProjectNodeMetadata> GetOutputs();
        public string? GetName(DialogProjectNodePort port);
    }
}
