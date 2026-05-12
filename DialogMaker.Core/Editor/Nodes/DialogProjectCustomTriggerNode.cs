using DialogMaker.Core.Editor.Collections;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DialogMaker.Core.Editor.Nodes
{
    public class DialogProjectCustomTriggerNode : DialogProjectTriggerNodeBase
    {
        public DialogProjectCustomTriggerNode(DialogProjectDialog dialog)
            : base(dialog)
        {
        }
        public DialogProjectCustomTriggerNode(DialogProjectDialog dialog, DialogProjectDialogNodeSavedState savedState)
            : base(dialog, savedState)
        {
        }

        public override DialogNodeType NodeType => DialogNodeType.CustomTrigger;
        [Name("Шаблон события")]
        public DialogProjectTriggerPreset? TriggerPreset
        {
            get => field;
            set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(TriggerPreset));
                    var oldValue = field;
                    field = value;
                    SetTriggerPreset(oldValue, value);
                    OnPropertyChanged(nameof(TriggerPreset));
                }
            }
        }

        protected override string? TriggerName => TriggerPreset?.Id;

        private readonly Dictionary<DialogProjectTriggerPresetPort, DialogProjectNodePort> _presetPorts = [];

        #region Управление

        protected override void ModifySavedState(DialogProjectDialogNodeSavedState savedState)
        {
            base.ModifySavedState(savedState);

            var triggerPreset = TriggerPreset;

            if (triggerPreset != null)
            {
                var reference = DialogProjectReference.Create(triggerPreset);
                savedState.Properties.TryAdd(nameof(TriggerPreset), reference.Save());
            }
        }
        protected override void Restore(DialogProjectDialogNodeSavedState savedState)
        {
            var reference = savedState.RestoreReference<DialogProjectTriggerPreset>(Project, nameof(TriggerPreset));

            if (reference != null)
            {
                try
                {
                    TriggerPreset = reference.Resolve();
                }
                catch (Exception error)
                {
                    Logger.Log(error);
                }
            }

            base.Restore(savedState);
        }

        private void SetTriggerPreset(DialogProjectTriggerPreset? oldValue, DialogProjectTriggerPreset? newValue)
        {
            if (oldValue != null)
            {
                oldValue.PropertyChanged -= OnTriggerPresetPropertyChanged;
                oldValue.Inputs.CollectionChanged -= OnInputsCollectionChanged;
                oldValue.Outputs.CollectionChanged -= OnOutputsCollectionChanged;
            }

            UpdatePreset(newValue);

            if (newValue == null)
            {
                return;
            }

            newValue.PropertyChanged += OnTriggerPresetPropertyChanged;
            newValue.Inputs.CollectionChanged += OnInputsCollectionChanged;
            newValue.Outputs.CollectionChanged += OnOutputsCollectionChanged;
        }

        private void Clear()
        {
            Clear(_ => true);
        }
        private void Clear(Predicate<DialogProjectNodePort> predicate)
        {
            foreach (var info in _presetPorts)
            {
                if (!predicate(info.Value))
                {
                    continue;
                }

                ClearPortPreset(info.Key);
                RemoveExtraPort(info.Value);
            }

            _presetPorts.Clear();
        }
        private void UpdatePreset(DialogProjectTriggerPreset? preset)
        {
            Clear();

            Name = preset?.Id ?? string.Empty;

            if (preset == null)
            {
                return;
            }

            UpdateInput(preset.Inputs);
            UpdateOutput(preset.Outputs);
        }
        private void UpdateInput(IEnumerable<DialogProjectTriggerPresetPort>? presets)
        {
            if (presets == null)
            {
                return;
            }

            foreach (var preset in presets)
            {
                UpdateInput(preset);
            }
        }
        private void UpdateInput(DialogProjectTriggerPresetPort preset)
        {
            Action<DialogProjectNodeInput>? extraHandler = null;

            if (preset.ValueType.HasFlag(AllowedObjectValues.Resource))
            {
                extraHandler = port =>
                {
                    if (port is DialogProjectNodeInputValue inputValuePort)
                    {
                        inputValuePort.SetAllowedValues(preset.AllowedValues);
                    }
                };
            }

            try
            {
                UpdatePortPreset(ExtraInputs, id =>
                {
                    return preset.ValueType switch
                    {
                        AllowedObjectValues.Number => new DialogProjectNodeInputNumber(this, id),
                        AllowedObjectValues.String => new DialogProjectNodeInputString(this, id),
                        AllowedObjectValues.Bool => new DialogProjectNodeInputBool(this, id),
                        _ => new DialogProjectNodeInputValue(this, id),
                    };
                }, extraHandler, port =>
                {
                    return preset.ValueType switch
                    {
                        AllowedObjectValues.Number => port is DialogProjectNodeInputNumber,
                        AllowedObjectValues.String => port is DialogProjectNodeInputString,
                        AllowedObjectValues.Bool => port is DialogProjectNodeInputBool,
                        _ => port is DialogProjectNodeInputValue &&
                             port is not DialogProjectNodeInputNumber &&
                             port is not DialogProjectNodeInputString &&
                             port is not DialogProjectNodeInputBool,
                    };
                }, OnInputPortPresetPropertyChanged, preset);
            }
            catch (Exception error)
            {
                Logger.Log(error);
            }
        }
        private void UpdateOutput(IEnumerable<DialogProjectTriggerPresetPort>? presets)
        {
            if (presets == null)
            {
                return;
            }

            foreach (var preset in presets)
            {
                UpdateOutput(preset);
            }
        }
        private void UpdateOutput(DialogProjectTriggerPresetPort preset)
        {
            try
            {
                UpdatePortPreset(ExtraOutputs, id =>
                {
                    return preset.ValueType switch
                    {
                        AllowedObjectValues.Number => new DialogProjectNodeOutputNumber(this, id),
                        AllowedObjectValues.String => new DialogProjectNodeOutputString(this, id),
                        AllowedObjectValues.Bool => new DialogProjectNodeOutputBool(this, id),
                        _ => new DialogProjectNodeOutputObject(this, id),
                    };
                }, null, port =>
                {
                    return preset.ValueType switch
                    {
                        AllowedObjectValues.Number => port is DialogProjectNodeOutputNumber,
                        AllowedObjectValues.String => port is DialogProjectNodeOutputString,
                        AllowedObjectValues.Bool => port is DialogProjectNodeOutputBool,
                        _ => port is DialogProjectNodeOutputObject &&
                             port is not DialogProjectNodeOutputNumber &&
                             port is not DialogProjectNodeOutputString &&
                             port is not DialogProjectNodeOutputBool,
                    };
                }, OnOutputPortPresetPropertyChanged, preset);
            }
            catch (Exception error)
            {
                Logger.Log(error);
            }
        }

        private void RemovePort(DialogProjectTriggerPresetPort preset)
        {
            if (!_presetPorts.TryGetValue(preset, out var port))
            {
                return;
            }

            _presetPorts.Remove(preset);

            RemoveExtraPort(port);
            ClearPortPreset(preset);
        }
        private void ClearPortPreset(DialogProjectTriggerPresetPort preset)
        {
            preset.PropertyChanged -= OnInputPortPresetPropertyChanged;
            preset.PropertyChanged -= OnOutputPortPresetPropertyChanged;
        }

        private DialogProjectNodeMetadata CreateMetadata(DialogProjectTriggerPresetPort preset)
        {
            return new(preset.Name ?? string.Empty);
        }
        private void UpdatePortPreset<TPort>(ObservableListAsDictionary<TPort, DialogProjectNodeMetadata> extraPorts,
                                             Func<int, TPort> portsFabric,
                                             Action<TPort>? extraHandler,
                                             Predicate<TPort> existedPortPredicate,
                                             PropertyChangedEventHandler propertyChangedHandler,
                                             DialogProjectTriggerPresetPort preset)
            where TPort : DialogProjectNodePort
        {
            bool needToAdd = false;

            _presetPorts.TryGetValue(preset, out var somePort);
            TPort? typedPort = null;
            int currentPortIndex = -1;

            if (somePort is TPort typedPortCheck)
            {
                typedPort = typedPortCheck;
            }
            if ((typedPort == null && somePort != null) ||
                (typedPort != null && !existedPortPredicate(typedPort)))
            {
                if (typedPort != null)
                {
                    currentPortIndex = extraPorts.Keys.IndexOf(typedPort);
                }

                RemovePort(preset);
                somePort = null;
                typedPort = null;
            }

            if (typedPort == null)
            {
                int id = GetNextExtraPortId();
                needToAdd = true;
                typedPort = portsFabric(id);

                if (typedPort is IValuePort valuePort &&
                    valuePort.CanPresetValue &&
                    preset.Value != null)
                {
                    valuePort.Value = preset.Value;
                }

                preset.PropertyChanged += propertyChangedHandler;
            }

            extraHandler?.Invoke(typedPort);

            var metadata = CreateMetadata(preset);
            typedPort.Name = preset.Name ?? DialogProjectTriggerPresetPort.DefaultName;

            if (needToAdd)
            {
                _presetPorts.Add(preset, typedPort);
            }

            if (currentPortIndex != -1)
            {
                extraPorts.ValuesCollection.Insert(currentPortIndex, new(typedPort, metadata));
                return;
            }
            if (!extraPorts.TryAdd(typedPort, metadata))
            {
                extraPorts[typedPort] = metadata;
            }
        }

        #endregion

        #region События

        private void OnTriggerPresetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is DialogProjectTriggerPreset preset &&
                e.PropertyName == nameof(Id))
            {
                Name = preset.Id;
            }
        }
        private void OnInputPortPresetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(TriggerPreset) &&
                sender is DialogProjectTriggerPresetPort port)
            {
                UpdateInput(port);
            }
        }
        private void OnOutputPortPresetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(TriggerPreset) &&
                sender is DialogProjectTriggerPresetPort port)
            {
                UpdateOutput(port);
            }
        }
        private void OnOutputsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdatePortsCollection(e, ExtraOutputs, UpdateOutput, t => t?.Outputs);
        }
        private void OnInputsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdatePortsCollection(e, ExtraInputs, UpdateInput, t => t?.Inputs);
        }

        private void UpdatePortsCollection<T>(NotifyCollectionChangedEventArgs e,
                                              ObservableListAsDictionary<T, DialogProjectNodeMetadata> ports,
                                              Action<DialogProjectTriggerPresetPort> updatePortMethod,
                                              Func<DialogProjectTriggerPreset?, IEnumerable<DialogProjectTriggerPresetPort>?> portPresetsSelector)
        {
            void Update(IList? items)
            {
                if (items == null)
                {
                    return;
                }

                foreach (var item in items)
                {
                    if (item is DialogProjectTriggerPresetPort port)
                    {
                        updatePortMethod(port);
                    }
                }
            }
            void Remove(IList? items)
            {
                if (items == null)
                {
                    return;
                }

                foreach (var item in items)
                {
                    if (item is DialogProjectTriggerPresetPort port)
                    {
                        RemovePort(port);
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Update(e.NewItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                Remove(e.OldItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                Remove(e.OldItems);
                Update(e.NewItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                if (e.OldStartingIndex == -1 || e.NewStartingIndex == -1)
                {
                    return;
                }

                var itemToMove = ports.ValuesCollection[e.OldStartingIndex];
                var itemAtDestinationIndex = ports.ValuesCollection[e.NewStartingIndex];

                ports.ValuesCollection[e.OldStartingIndex] = default;
                ports.ValuesCollection[e.NewStartingIndex] = default;
                ports.ValuesCollection[e.OldStartingIndex] = itemAtDestinationIndex;
                ports.ValuesCollection[e.NewStartingIndex] = itemToMove;
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Clear(p => p is DialogProjectNodeOutput);
                UpdateOutput(portPresetsSelector(TriggerPreset));
            }
        }

        #endregion
    }
}
