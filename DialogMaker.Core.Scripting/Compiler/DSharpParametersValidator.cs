using DialogMaker.Core.Scripting.Compiler.Builders;
using DialogMaker.Core.Scripting.Runtime;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace DialogMaker.Core.Scripting.Compiler
{
    /// <summary>
    /// Validator for <see cref="IDSharpParameterInfo"/> and <see cref="DSharpMethodBuilderParameter"/>
    /// </summary>
    public class DSharpParametersValidator : IDisposable
    {
        ~DSharpParametersValidator()
        {
            Dispose();
        }

        /// <summary>
        /// Is validator disposed
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// List of parameter builders for observing and validating
        /// </summary>
        public IList<DSharpMethodBuilderParameter>? Parameters
        {
            get;
            set
            {
                if (field == value)
                {
                    return;
                }

                ObjectDisposedException.ThrowIf(IsDisposed, this);

                if (field is INotifyCollectionChanged oldObservableCollection)
                {
                    oldObservableCollection.CollectionChanged -= OnParametersCollectionChanged;
                }
                if (value is INotifyCollectionChanged newObservableCollection)
                {
                    newObservableCollection.CollectionChanged += OnParametersCollectionChanged;
                }

                field = value;
            }
        }
        /// <summary>
        /// List of allowed parameter modes
        /// </summary>
        public List<DSharpMethodParameterMode> AllowedModes { get; } = [];

        private readonly HashSet<IDSharpParameterInfo> _observedParameters = [];

        #region Controls

        /// <summary>
        /// Validate parameter
        /// </summary>
        /// <param name="parameter">Parameter that need to validate</param>
        /// <returns>Is parameter successfully passed validation</returns>
        public bool Validate(IDSharpParameterInfo parameter)
        {
            return AllowedModes.Contains(parameter.Mode);
        }
        /// <summary>
        /// Validate parameter and throw exception when validation not passed
        /// </summary>
        /// <param name="parameter">Parameter that need to validate</param>
        /// <exception cref="ArgumentException"></exception>
        [StackTraceHidden]
        public void ValidateWithException(IDSharpParameterInfo parameter)
        {
            if (!Validate(parameter))
            {
                throw new ArgumentException($"Parameter mode \"{parameter.Mode}\" is not available for current member");
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            ClearParameters();
            GC.SuppressFinalize(this);
        }

        private void ClearParameters()
        {
            foreach (var parameter in _observedParameters)
            {
                if (parameter is INotifyPropertyChanged observable)
                {
                    observable.PropertyChanged -= OnParameterPropertyChanged;
                }
            }

            _observedParameters.Clear();
        }
        private bool AddParameter(IDSharpParameterInfo parameter)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);

            if (parameter is not INotifyPropertyChanged observable ||
                !_observedParameters.Add(parameter))
            {
                return false;
            }

            observable.PropertyChanged += OnParameterPropertyChanged;

            return true;
        }
        private bool RemoveParameter(IDSharpParameterInfo parameter)
        {
            if (parameter is not INotifyPropertyChanged observable ||
                !_observedParameters.Remove(parameter))
            {
                return false;
            }

            observable.PropertyChanged -= OnParameterPropertyChanged;

            return true;
        }

        #endregion

        #region Events

        private void OnParametersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            void Add(IList? items)
            {
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item is IDSharpParameterInfo parameter)
                        {
                            AddParameter(parameter);
                            ValidateWithException(parameter);
                        }
                    }
                }
            }
            void Remove(IList? items)
            {
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item is IDSharpParameterInfo parameter)
                        {
                            RemoveParameter(parameter);
                        }
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Add(e.NewItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                Remove(e.OldItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                Add(e.NewItems);
                Remove(e.OldItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ClearParameters();
            }
        }
        private void OnParameterPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Mode" && sender is IDSharpParameterInfo parameter)
            {
                ValidateWithException(parameter);
            }
        }

        #endregion

        #region Static

        /// <summary>
        /// Create validator with all allowed modes
        /// </summary>
        /// <returns>Parameters validator</returns>
        public static DSharpParametersValidator CreateWithAllModes()
        {
            DSharpParametersValidator validator = new();
            validator.AllowedModes.AddRange(DSharpMethodParameterModeHelper.Values);

            return validator;
        }
        /// <summary>
        /// Create validator with modes for indexers and constructors
        /// </summary>
        /// <returns>Parameters validator</returns>
        public static DSharpParametersValidator CreateWithSimpleModes()
        {
            DSharpParametersValidator validator = CreateWithAllModes();
            validator.AllowedModes.Remove(DSharpMethodParameterMode.Out);
            validator.AllowedModes.Remove(DSharpMethodParameterMode.This);

            return validator;
        }

        #endregion
    }
}
