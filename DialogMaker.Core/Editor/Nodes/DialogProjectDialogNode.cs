using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;

namespace DialogMaker.Core.Editor.Nodes
{
    public abstract class DialogProjectDialogNode : ObservableObject, INode, ISavable
    {
        protected DialogProjectDialogNode(DialogProjectDialog dialog)
        {
            Dialog = dialog;
            Id = Guid.NewGuid();
        }
        protected DialogProjectDialogNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
        {
            Id = Guid.Parse(savedState.Id);
            Dialog = dialog;
            Position = savedState.Position;
        }

        public DialogProjectDialog Dialog { get; }
        public Guid Id { get; }
        public abstract DialogNodeType NodeType { get; }
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    InvokePropertyChanged(nameof(Position));
                }
            }
        }
        public IPortDataConverter DataConverter => DialogProjectPortDataConverter.Instance;

        private Vector2 _position;

        #region Управление

        public DialogProjectDialogNodeSavedState Save()
        {
            var savedState = CreateSavedState();
            savedState.Id = Id.ToString();
            savedState.NodeType = NodeType;
            savedState.Position = Position;

            return savedState;
        }

        ISavedState ISavable.Save() => Save();

        protected abstract DialogProjectDialogNodeSavedState CreateSavedState();

        #endregion

        #region Статика

        public static ReadOnlyDictionary<DialogNodeType, Type> AvailableNodes
        {
            get
            {
                if (field == null)
                {
                    Dictionary<DialogNodeType, Type> result = [];

                    foreach (var value in Enum.GetValues(typeof(DialogNodeType)))
                    {
                        var node = value.GetEnumAttribute<NodeAttribute>();

                        if (node != null)
                        {
                            result.Add((DialogNodeType)value, node.NodeType);
                        }
                    }

                    field = new(result);
                } 

                return field;
            }
        }

        public static DialogProjectDialogNode Create(DialogProjectDialog dialog, DialogNodeType type)
        {
            if (AvailableNodes.TryGetValue(type, out var nodeType))
            {
                return (DialogProjectDialogNode)Activator.CreateInstance(nodeType, dialog);
            }

            throw new ArgumentException($"Узел недоступен: {type}", nameof(type));
        }
        public static DialogProjectDialogNode Restore(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
        {
            if (AvailableNodes.TryGetValue(savedState.NodeType, out var nodeType))
            {
                return (DialogProjectDialogNode)Activator.CreateInstance(nodeType, dialog, savedState);
            }

            throw new ArgumentException($"Узел недоступен: {savedState.NodeType}", nameof(savedState));
        }

        #endregion
    }
}
