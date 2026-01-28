using DialogMaker.Core.Common;
using DialogMaker.Core.Executioning;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;

namespace DialogMaker.Core.Editor.Nodes
{
    public abstract class DialogProjectDialogNode : Disposable, INode, ISavable
    {
        protected DialogProjectDialogNode(DialogProjectDialog dialog)
        {
            Dialog = dialog;
            Id = Guid.NewGuid();
        }
        protected DialogProjectDialogNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
        {
            Id = savedState.Id;
            Dialog = dialog;
            Position = savedState.Position;

            Restore(savedState);
        }

        public DialogProject Project => Pack.Project;
        public DialogProjectPack Pack => Dialog.Pack;
        public DialogProjectDialog Dialog { get; }
        public Guid Id { get; }
        public string Name
        {
            get
            {
                if (field == null)
                {
                    field = NodeType.GetEnumAttribute<NameAttribute>()?.Name;
                    field ??= NodeType.ToString();
                }

                return field;
            }
        }
        public string Description
        {
            get
            {
                if (field == null)
                {
                    field = NodeType.GetEnumAttribute<DescriptionAttribute>()?.Description;
                    field ??= string.Empty;
                }

                return field;
            }
        }
        public abstract DialogNodeType NodeType { get; }
        public Vector2 Position
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Position));
                    field = value;
                    InvokePropertyChanged(nameof(Position));
                }
            }
        }
        public IPortDataConverter DataConverter => DialogProjectPortDataConverter.Instance;
        public virtual bool IsUserHandleNode { get; }
        public virtual bool IsCodeGenerator => true;
        public virtual bool IsSystem => false;
        public bool IsSeparator
        {
            get
            {
                if (!IsCodeGenerator)
                {
                    return false;
                }

                int connectedNodes = 0;

                foreach (var output in GetOutputs().Keys)
                {
                    connectedNodes += output.ConnectionsCount;
                }

                return connectedNodes > 1;
            }
        }
        public bool IsFunction
        {
            get
            {
                if (IsSystem)
                {
                    return true;
                }

                foreach (var input in GetInputs().Keys)
                {
                    if (input.ConnectionsCount > 1)
                    {
                        return true;
                    }
                    foreach (var connection in input)
                    {
                        if (connection.Node.IsSeparator)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
        public bool IsImmediate
        {
            get
            {
                foreach (var input in GetInputs().Keys)
                {
                    if (input.ConnectionsCount > 0 &&
                        input.ConnectionType == DialogNodeConnectionType.Action)
                    {
                        return false;
                    }
                    foreach (var connection in input)
                    {
                        if (!connection.Node.IsImmediate)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }
        public bool CanBeEntryPoint
        {
            get
            {
                foreach (var input in GetInputs().Keys)
                {
                    foreach (var connection in input.Connections)
                    {
                        if (connection.Node.IsUserHandleNode)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        private ReadOnlyDictionary<DialogProjectNodeInput, DialogProjectNodeMetadata>? _inputs;
        private ReadOnlyDictionary<DialogProjectNodeOutput, DialogProjectNodeMetadata>? _outputs;

        #region Управление

        public IEnumerable<INode> GetLocalGroup(Predicate<INode> ignorePredicate)
        {
            if (IsFunction && !IsSeparator)
            {
                yield return this;

                foreach (var output in GetOutputs().Keys)
                {
                    foreach (var connection in output)
                    {
                        if (ignorePredicate(connection.Node) ||
                            connection.Node.IsFunction)
                        {
                            continue;
                        }

                        var nextGroup = connection.Node.GetLocalGroup(n =>
                        {
                            return n == this || ignorePredicate(n);
                        });

                        foreach (var groupNode in nextGroup)
                        {
                            yield return groupNode;
                        }
                    }
                }

                yield break;
            }

            foreach (var input in GetInputs().Keys)
            {
                foreach (var connection in input)
                {
                    if (ignorePredicate(connection.Node) ||
                        connection.Node.IsSeparator)
                    {
                        continue;
                    }

                    var subGroup = connection.Node.GetLocalGroup(n =>
                    {
                        return n == this || ignorePredicate(n);
                    });

                    foreach (var subNode in subGroup)
                    {
                        yield return subNode;
                    }
                }
            }

            yield return this;
        }

        public abstract void Compile(DialogCompilerContext context);

        public IEnumerable<DialogProjectNodePort> GetPorts(Predicate<DialogProjectNodePort> predicate)
        {
            foreach (var port in GetInputs().Keys)
            {
                if (predicate(port))
                {
                    yield return port;
                }
            }
            foreach (var port in GetOutputs().Keys)
            {
                if (predicate(port))
                {
                    yield return port;
                }
            }
        }
        public IEnumerable<DialogProjectNodePort> GetPorts()
        {
            return GetPorts(p => true);
        }
        public ReadOnlyDictionary<DialogProjectNodeInput, DialogProjectNodeMetadata> GetInputs()
        {
            if (_inputs == null)
            {
                var ports = GetPorts<DialogProjectNodeInput, NodeInputAttribute>(this);
                _inputs = new(ports);
            }

            return _inputs;
        }
        public ReadOnlyDictionary<DialogProjectNodeOutput, DialogProjectNodeMetadata> GetOutputs()
        {
            if (_outputs == null)
            {
                var ports = GetPorts<DialogProjectNodeOutput, NodeOutputAttribute>(this);
                _outputs = new(ports);
            }

            return _outputs;
        }
        public bool TryGetPort(int id, [NotNullWhen(true)] out DialogProjectNodePort? result)
        {
            bool Search(IEnumerable<DialogProjectNodePort> ports, [NotNullWhen(true)] out DialogProjectNodePort? result)
            {
                result = null;

                foreach (var port in ports)
                {
                    if (port.Id == id)
                    {
                        result = port;
                        return true;
                    }
                }

                return false;
            }

            if (Search(GetInputs().Keys, out result) ||
                Search(GetOutputs().Keys, out result))
            {
                return true;
            }

            return false;
        }
        public virtual bool TryGetResourceValue(DialogProjectNodeOutput port, [NotNullWhen(true)] out IResourceItem? resource)
        {
            resource = null;
            return false;
        }
        public string? GetName(DialogProjectNodePort port)
        {
            foreach (var info in GetOutputs())
            {
                if (info.Key.Equals(port))
                {
                    return info.Value.Name;
                }
            }
            foreach (var info in GetInputs())
            {
                if (info.Key.Equals(port))
                {
                    return info.Value.Name;
                }
            }

            return string.Empty;
        }

        public DialogProjectDialogNodeSavedState Save()
        {
            CheckHelper.CheckIsDisposed(this);

            var savedState = CreateSavedState();
            ModifySavedState(savedState);

            savedState.Id = Id;
            savedState.NodeType = NodeType;
            savedState.Position = Position;

            foreach (var port in GetInputs().Keys)
            {
                savedState.Inputs.Add(port.Id, port.Save());
            }
            foreach (var port in GetOutputs().Keys)
            {
                savedState.Outputs.Add(port.Id, port.Save());
            }

            return savedState;
        }

        ISavedState ISavable.Save() => Save();

        protected virtual DialogProjectDialogNodeSavedState CreateSavedState()
        {
            return new();
        }
        protected virtual void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
        }
        protected virtual void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            static void Restore(IEnumerable<DialogProjectNodePort> ports, Dictionary<int, DialogProjectNodePortSavedState> states)
            {
                foreach (var port in ports)
                {
                    if (states.TryGetValue(port.Id, out var state))
                    {
                        try
                        {
                            port.Restore(state);
                        }
                        catch (Exception error)
                        {
                            Debug.WriteLine(error);
                        }
                    }
                }
            }

            Restore(GetInputs().Keys, savedState.Inputs);
            Restore(GetOutputs().Keys, savedState.Inputs);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            static void DisposePorts(IEnumerable<DialogProjectNodePort> ports)
            {
                foreach (var port in ports)
                {
                    port.Dispose();
                }
            }

            DisposePorts(GetInputs().Keys);
            DisposePorts(GetOutputs().Keys);

            _inputs = null;
            _outputs = null;
        }

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

        private static Dictionary<TPort, DialogProjectNodeMetadata> GetPorts<TPort, T>(DialogProjectDialogNode node)
            where TPort : DialogProjectNodePort
            where T : NameAttribute
        {
            Dictionary<TPort, DialogProjectNodeMetadata> portsMetadata = [];
            SortedDictionary<SortAttribute, List<TPort>> sortedPorts = [];
            List<TPort> otherPorts = [];

            foreach (var property in node.GetType().GetProperties())
            {
                var attribute = property.GetCustomAttribute<T>();
                var sortAttribute = property.GetCustomAttribute<SortAttribute>();

                if (attribute == null ||
                    property.GetValue(node) is not TPort port)
                {
                    continue;
                }

                string name = attribute.Name;
                string description = property.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
                DialogProjectNodeMetadata metadata = new(name, description);
                port._name = name;

                portsMetadata.Add(port, metadata);

                if (sortAttribute != null)
                {
                    if (sortedPorts.TryGetValue(sortAttribute, out var ports))
                    {
                        ports.Add(port);
                        continue;
                    }

                    sortedPorts.Add(sortAttribute, [port]);
                }
                else
                {
                    otherPorts.Add(port);
                }
            }

            otherPorts.Sort();
            Dictionary<TPort, DialogProjectNodeMetadata> result = [];

            foreach (var list in sortedPorts.Values)
            {
                foreach (var port in list)
                {
                    result.Add(port, portsMetadata[port]);
                }
            }
            foreach (var port in otherPorts)
            {
                result.Add(port, portsMetadata[port]);
            }

            return result;
        }

        internal static void CompileMath(DialogCompilerContext context, DialogByteCode code, DialogProjectNodeInputValue firstValue, DialogProjectNodeInputValue secondValue, DialogProjectNodeOutputObject outputPort)
        {
            var value1 = context.Compiler.RecursiveCompileConnections(context, firstValue);
            var value2 = context.Compiler.RecursiveCompileConnections(context, secondValue);
            var output = context.Resources.GetOrCreateVariable(outputPort);

            var setToOutput = context.Section.CreateOperation(DialogByteCode.Set);
            setToOutput.Arguments[0] = output;
            setToOutput.Arguments[1] = value1;

            var mathOpCode = context.Section.CreateOperation(code);
            mathOpCode.Arguments[0] = output;
            mathOpCode.Arguments[1] = value2;


            context.CompileOutputs(outputPort);
        }

        #endregion
    }
}
