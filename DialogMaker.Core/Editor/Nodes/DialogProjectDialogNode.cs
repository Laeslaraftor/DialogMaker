using Acly;
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
using System.Collections.Specialized;

namespace DialogMaker.Core.Editor.Nodes
{
    public abstract class DialogProjectDialogNode : Disposable, INode, ISavable
    {
        protected DialogProjectDialogNode(DialogProjectDialog dialog)
            : this(dialog, Guid.NewGuid())
        {
        }
        protected DialogProjectDialogNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : this(dialog, savedState.Id)
        {
            Position = savedState.Position;
            Inverted = savedState.Inverted;

            try
            {
                Restore(savedState);
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
            }
        }
        private DialogProjectDialogNode(DialogProjectDialog dialog, Guid id)
        {
            Dialog = dialog;
            Id = id;

            ExtraInputs.CollectionChanged += OnExtraInputsCollectionChanged;
            ExtraOutputs.CollectionChanged += OnExtraOutputsCollectionChanged;
        }

        public event EventHandler? PortsUpdated;
        public event EventHandler? InputsUpdated;
        public event EventHandler? OutputsUpdated;

        public DialogProject Project => Pack.Project;
        public DialogProjectPack Pack => Dialog.Pack;
        public DialogProjectDialog Dialog { get; }
        public IResourcesOwner Owner => Dialog;
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

                var inputs = GetInputs().Keys;

                foreach (var input in inputs)
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
        public bool Inverted
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Inverted));
                    field = value;
                    InvokePropertyChanged(nameof(Inverted));
                }
            }
        }

        // дополнительные порты имеют отрицательные идетификаторы
        protected ObservableDictionary<DialogProjectNodeInput, DialogProjectNodeMetadata> ExtraInputs { get; } = [];
        protected ObservableDictionary<DialogProjectNodeOutput, DialogProjectNodeMetadata> ExtraOutputs { get; } = [];

        private Dictionary<DialogProjectNodeInput, DialogProjectNodeMetadata>? _baseInputs;
        private Dictionary<DialogProjectNodeOutput, DialogProjectNodeMetadata>? _baseOutputs;
        private ObservableDictionary<DialogProjectNodeInput, DialogProjectNodeMetadata>? _dynamicInputs;
        private ObservableDictionary<DialogProjectNodeOutput, DialogProjectNodeMetadata>? _dynamicOutputs;

        private ReferenceReadOnlyDictionary<DialogProjectNodeInput, DialogProjectNodeMetadata>? _readInputs;
        private ReferenceReadOnlyDictionary<DialogProjectNodeOutput, DialogProjectNodeMetadata>? _readOutputs;

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
        public ReferenceReadOnlyDictionary<DialogProjectNodeInput, DialogProjectNodeMetadata> GetInputs()
        {
            if (_baseInputs == null)
            {
                var ports = GetPorts<DialogProjectNodeInput, NodeInputAttribute>(this);
                _baseInputs = new(ports);
            }

            _dynamicInputs ??= CombineDictionaries(_baseInputs, ExtraInputs);
            _readInputs ??= new(_dynamicInputs);

            return _readInputs;
        }
        public ReferenceReadOnlyDictionary<DialogProjectNodeOutput, DialogProjectNodeMetadata> GetOutputs()
        {
            if (_baseOutputs == null)
            {
                var ports = GetPorts<DialogProjectNodeOutput, NodeOutputAttribute>(this);
                _baseOutputs = new(ports);
            }

            _dynamicOutputs ??= CombineDictionaries(_baseOutputs, ExtraOutputs);
            _readOutputs ??= new(_dynamicOutputs);

            return _readOutputs;
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
            savedState.Inverted = Inverted;

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

        protected IEnumerable<DialogProjectNodePort> GetExtraPorts()
        {
            foreach (var port in ExtraInputs.Keys)
            {
                yield return port;
            }
            foreach (var port in ExtraOutputs.Keys)
            {
                yield return port;
            }
        }
        protected int GetNextExtraPortId()
        {
            int extraPortsCount = -(ExtraOutputs.Count + ExtraInputs.Count);

            if (extraPortsCount == 0)
            {
                return -1;
            }

            var extraPorts = GetExtraPorts();

            for (int i = -1; i > extraPortsCount; i--)
            {
                bool failed = false;

                foreach (var port in extraPorts)
                {
                    if (port.Id == i)
                    {
                        failed = true;
                        break;
                    }
                }

                if (!failed)
                {
                    return i;
                }
            }

            return extraPortsCount - 1;
        }

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

            ExtraInputs.CollectionChanged -= OnExtraInputsCollectionChanged;
            ExtraOutputs.CollectionChanged -= OnExtraOutputsCollectionChanged;

            static void DisposePorts(IEnumerable<DialogProjectNodePort> ports)
            {
                foreach (var port in ports)
                {
                    port.Dispose();
                }
            }

            DisposePorts(GetInputs().Keys);
            DisposePorts(GetOutputs().Keys);

            _baseInputs = null;
            _baseOutputs = null;
        }

        #endregion

        #region События

        private void OnExtraOutputsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateDynamicDictionary(_dynamicOutputs, _baseOutputs, ExtraOutputs);

            OutputsUpdated?.Invoke(this, e);
            PortsUpdated?.Invoke(this, e);
        }
        private void OnExtraInputsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateDynamicDictionary(_dynamicInputs, _baseInputs, ExtraInputs);

            InputsUpdated?.Invoke(this, e);
            PortsUpdated?.Invoke(this, e);
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
        private static ObservableDictionary<TPort, DialogProjectNodeMetadata> CombineDictionaries<TPort>(Dictionary<TPort, DialogProjectNodeMetadata> main, ObservableDictionary<TPort, DialogProjectNodeMetadata> extraPorts)
            where TPort : DialogProjectNodePort
        {
            ObservableDictionary<TPort, DialogProjectNodeMetadata> result = [.. main];

            foreach (var extra in extraPorts)
            {
                result.Add(extra);
            }

            return result;
        }
        private static void UpdateDynamicDictionary<TPort>(ObservableDictionary<TPort, DialogProjectNodeMetadata>? dynamic, Dictionary<TPort, DialogProjectNodeMetadata>? main, ObservableDictionary<TPort, DialogProjectNodeMetadata>? extra)
            where TPort : DialogProjectNodePort
        {
            if (dynamic == null || main == null || extra == null)
            {
                return;
            }

            dynamic.Clear();

            foreach (var info in main)
            {
                dynamic.Add(info);
            }
            foreach (var info in extra)
            {
                dynamic.Add(info);
            }
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
