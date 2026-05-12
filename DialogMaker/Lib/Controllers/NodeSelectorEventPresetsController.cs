using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor;
using DialogMaker.Lib.Data;
using DialogMaker.Lib.Elements;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Lib.Controllers
{
    public class NodeSelectorEventPresetsController : Disposable
    {
        public NodeSelectorEventPresetsController()
        {
            RootEventPresets = new()
            {
                IsEnabled = true,
                Name = "События",
                IsContainer = true,
                Children = _itemsContainer
            };
        }

        public NodeSelectorItemInfo RootEventPresets { get; }
        public ProjectResources? Resources
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(Resources));
                    field?.InheritedTriggerPresets.CollectionChanged -= OnTriggerPresetsCollectionChanged;
                    value?.InheritedTriggerPresets.CollectionChanged += OnTriggerPresetsCollectionChanged;
                    field = value;
                    UpdateList();
                    OnPropertyChanged(nameof(Resources));
                }
            }
        }

        private readonly EditableCollection<NodeSelectorItemInfo> _itemsContainer = [];
        private readonly ElementsPool<TriggerPresetNodeSelectorItemInfo> _itemsPool = new();
        private readonly Dictionary<PresetToken, TriggerPresetNodeSelectorItemInfo> _presetItems = [];
        private readonly Dictionary<ProjectTriggerPreset, List<RootItemInfo>> _presetRootItems = [];

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Clear();
        }

        private void Clear()
        {
            _itemsContainer.Clear();
            _itemsPool.Clear();

            foreach (var token in _presetItems.Keys)
            {
                if (token.TriggerPreset != null)
                {
                    ClearPreset(token.TriggerPreset);
                }
                else if (token.PortPreset != null)
                {
                    ClearPort(token.PortPreset);
                }
            }

            _presetItems.Clear();
        }
        private void UpdateList()
        {
            Clear();

            var resources = Resources;

            if (resources == null)
            {
                return;
            }

            foreach (var preset in resources.InheritedTriggerPresets)
            {
                CreateItem(preset);
            }

            _itemsContainer.Sort();
        }

        private void CreateItem(ProjectTriggerPreset preset)
        {
            if (_presetItems.ContainsKey(preset) ||
                _presetRootItems.ContainsKey(preset))
            {
                return;
            }

            void SetupItem(ProjectTriggerPreset preset, TriggerPresetNodeSelectorItemInfo item)
            {
                item.IsEnabled = true;
                item.Port = null;
                item.Value = TriggerNodeInfo;
                item.TriggerPreset = preset.Original;
            }

            var presetItem = _itemsPool.GetElement();
            var inputItem = _itemsPool.GetElement();
            var outputItem = _itemsPool.GetElement();
            List<RootItemInfo> rootItems = [
                new(presetItem, null),
                new(inputItem, DialogNodePortDirection.Input),
                new(outputItem, DialogNodePortDirection.Output)
            ];

            foreach (var info in rootItems)
            {
                SetupItem(preset, info.Item);
                UpdateItem(preset, info);
                _itemsContainer.Add(info.Item);
            }

            _presetItems.Add(preset, presetItem);
            _presetRootItems.Add(preset, rootItems);

            void CreatePorts(DialogNodePortDirection direction, IEnumerable<DialogProjectTriggerPresetPort> ports)
            {
                foreach (var port in ports)
                {
                    AddPort(direction, port);
                }
            }

            CreatePorts(DialogNodePortDirection.Input, preset.Original.Inputs);
            CreatePorts(DialogNodePortDirection.Output, preset.Original.Outputs);

            preset.PropertyChanged += OnPresetPropertyChanged;
            preset.Original.Inputs.ItemChanged += OnPresetInputsItemChanged;
            preset.Original.Outputs.ItemChanged += OnPresetOutputsItemChanged;
        }
        private void RemoveItem(ProjectTriggerPreset preset)
        {
            if (!_presetRootItems.TryGetValue(preset, out var rootItems))
            {
                return;
            }

            _presetItems.Remove(preset);
            _presetRootItems.Remove(preset);

            foreach (var info in rootItems)
            {
                _itemsContainer.Remove(info.Item);
                _itemsPool.Free(info.Item);
            }

            void RemoveAll(IEnumerable<DialogProjectTriggerPresetPort> ports)
            {
                foreach (var port in ports)
                {
                    RemovePort(port);
                }
            }

            RemoveAll(preset.Original.Inputs);
            RemoveAll(preset.Original.Outputs);

            ClearPreset(preset);
        }
        private void UpdateItem(ProjectTriggerPreset preset, RootItemInfo info)
        {
            UpdateItem(preset, info.Item, info.Direction);
        }
        private void UpdateItem(ProjectTriggerPreset preset, NodeSelectorItemInfo item, DialogNodePortDirection? direction)
        {
            if (direction == null)
            {
                item.Name = preset.Id;
                return;
            }

            string portName = "Вход";

            if (direction == DialogNodePortDirection.Output)
            {
                portName = "Выход";
            }

            item.Name = $"{preset.Id} → {portName}";
            item.Port = NodeSelectorItemInfoPort.Create(direction.Value, DialogNodeConnectionType.Action, portName);
        }
        private void UpdateItem(DialogProjectTriggerPresetPort port, NodeSelectorItemInfo item, DialogNodePortDirection? direction = null)
        {
            item.Name = $"{port.TriggerPreset.Id} → {port.Name}";
            DialogNodePortDirection directionValue = DialogNodePortDirection.Input;

            if (direction != null)
            {
                directionValue = direction.Value;
            }
            else if (item.Port != null)
            {
                directionValue = item.Port.Value.Direction;
            }

            item.Port = NodeSelectorItemInfoPort.Create(directionValue, DialogNodeConnectionType.Data, port.Name ?? string.Empty);
        }

        private void AddPort(DialogNodePortDirection direction, DialogProjectTriggerPresetPort port)
        {
            var item = _itemsPool.GetElement();
            item.IsEnabled = true;
            item.Value = TriggerNodeInfo;
            item.TriggerPreset = port.TriggerPreset;
            UpdateItem(port, item, direction);

            port.PropertyChanged += OnPresetPortPropertyChanged;

            _presetItems.Add(port, item);
            _itemsContainer.Add(item);
        }
        private void RemovePort(DialogProjectTriggerPresetPort port)
        {
            ClearPort(port);

            if (!_presetItems.TryGetValue(port, out var item))
            {
                return;
            }

            _presetItems.Remove(port);
            _itemsContainer.Remove(item);
            _itemsPool.Free(item);
        }

        private void ClearPreset(ProjectTriggerPreset preset)
        {
            preset.PropertyChanged -= OnPresetPropertyChanged;
            preset.Original.Inputs.ItemChanged -= OnPresetInputsItemChanged;
            preset.Original.Outputs.ItemChanged -= OnPresetOutputsItemChanged;
        }
        private void ClearPort(DialogProjectTriggerPresetPort port)
        {
            port.PropertyChanged -= OnPresetPortPropertyChanged;
        }

        #endregion

        #region События

        private void OnTriggerPresetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems == null)
                {
                    return;
                }

                foreach (var item in e.NewItems)
                {
                    if (item is ProjectTriggerPreset preset)
                    {
                        CreateItem(preset);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems == null)
                {
                    return;
                }

                foreach (var item in e.OldItems)
                {
                    if (item is ProjectTriggerPreset preset)
                    {
                        RemoveItem(preset);
                    }
                }
            }
            else
            {
                UpdateList();
            }
        }
        private void OnPresetPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ProjectTriggerPreset preset)
            {
                return;
            }
            foreach (var info in _presetItems)
            {
                if (info.Key.TriggerPreset == preset &&
                    _presetRootItems.TryGetValue(preset, out var items))
                {
                    foreach (var item in items)
                    {
                        UpdateItem(preset, item);
                    }

                    _itemsContainer.Sort();
                }
            }
        }
        private void OnPresetPortPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is DialogProjectTriggerPresetPort port &&
                _presetItems.TryGetValue(port, out var item))
            {
                UpdateItem(port, item);
            }
        }
        private void OnPresetOutputsItemChanged(object? sender, CollectionItemEventArgs<DialogProjectTriggerPresetPort> e)
        {
            if (e.Action == CollectionItemAction.Remove)
            {
                RemovePort(e.Item);
                return;
            }
            if (e.Action == CollectionItemAction.Add)
            {
                AddPort(DialogNodePortDirection.Output, e.Item);
            }

            _itemsContainer.Sort();
        }
        private void OnPresetInputsItemChanged(object? sender, CollectionItemEventArgs<DialogProjectTriggerPresetPort> e)
        {
            if (e.Action == CollectionItemAction.Remove)
            {
                RemovePort(e.Item);
                return;
            }
            if (e.Action == CollectionItemAction.Add)
            {
                AddPort(DialogNodePortDirection.Input, e.Item);
            }

            _itemsContainer.Sort();
        }

        #endregion

        #region Статика

        private static DialogNodeInfo TriggerNodeInfo
        {
            get
            {
                _triggerNodeInfo ??= DialogProjectDialogNode.AvailableNodes[DialogNodeType.CustomTrigger];
                return _triggerNodeInfo.Value;
            }
        }

        private static DialogNodeInfo? _triggerNodeInfo;

        #endregion

        #region Структуры

        private readonly struct RootItemInfo(TriggerPresetNodeSelectorItemInfo item, DialogNodePortDirection? direction)
        {
            public TriggerPresetNodeSelectorItemInfo Item { get; } = item;
            public DialogNodePortDirection? Direction { get; } = direction;
        }
        private readonly struct PresetToken : IEquatable<PresetToken>
        {
            private PresetToken(ProjectTriggerPreset triggerPreset)
            {
                TriggerPreset = triggerPreset;
            }
            private PresetToken(DialogProjectTriggerPresetPort portPreset)
            {
                PortPreset = portPreset;
            }

            public ProjectTriggerPreset? TriggerPreset { get; }
            public DialogProjectTriggerPresetPort? PortPreset { get; }

            #region Управление

            public override int GetHashCode()
            {
                if (TriggerPreset != null)
                {
                    return TriggerPreset.GetHashCode();
                }
                else if (PortPreset != null)
                {
                    return PortPreset.GetHashCode();
                }

                return 0;
            }
            public bool Equals(PresetToken other)
            {
                if (other.TriggerPreset != null)
                {
                    return Equals((object)other.TriggerPreset);
                }
                else if (other.PortPreset != null)
                {
                    return Equals((object)other.PortPreset);
                }

                return false;
            }
            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                if (obj is PresetToken token)
                {
                    return Equals(token);
                }
                if (obj is DialogProjectTriggerPreset triggerPreset)
                {
                    return triggerPreset == TriggerPreset?.Original;
                }
                else if (obj is DialogProjectTriggerPresetPort portPreset)
                {
                    return portPreset == PortPreset;
                }
                else if (obj is ProjectTriggerPreset pTriggerPreset)
                {
                    return pTriggerPreset.Original == TriggerPreset?.Original;
                }
                else if (obj is ProjectTriggerPresetPort pPortPreset)
                {
                    return pPortPreset.Original == PortPreset;
                }

                return base.Equals(obj);
            }

            public static implicit operator PresetToken(DialogProjectTriggerPresetPort portPreset)
            {
                return new(portPreset);
            }
            public static implicit operator PresetToken(ProjectTriggerPreset triggerPreset)
            {
                return new(triggerPreset);
            }
            public static implicit operator PresetToken(ProjectTriggerPresetPort portPreset)
            {
                return new(portPreset.Original);
            }

            #endregion
        }

        #endregion
    }
}
