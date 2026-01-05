using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;
using System;
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
        public bool IsUserHandleNode { get; }
        public bool CanBeEntryPoint { get; }

        public void Compile(DialogCompilerContext context);
        public bool TryGetResourceValue(DialogProjectNodeOutput port, [NotNullWhen(true)] out IResourceItem? resource);
    }
}
