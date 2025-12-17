using System;
using System.ComponentModel;

namespace DialogMaker.Core.Editor.Nodes
{
    public interface INode : INotifyPropertyChanged, IDisposable
    {
        public bool IsDisposed { get; }
        public Guid Id { get; }
        public DialogNodeType NodeType { get; }
        public IPortDataConverter DataConverter { get; }
    }
}
