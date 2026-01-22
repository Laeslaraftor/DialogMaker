using DialogMaker.Core;
using DialogMaker.Lib;
using DialogMaker.Lib.Controllers;
using DialogMaker.Lib.Elements;
using System.ComponentModel;
using System.Windows;

namespace DialogMaker.Editor.Runtime
{
    public class DialogCompilerItem : Disposable, IItemTab, IActionsItemTab
    {
        public DialogCompilerItem(ProjectDialog dialog)
        {
            Dialog = dialog;
            _startStopButton = new()
            {
                Color = App.SuccessBrush,
                Icon = Icons.Play,
                Text = "Запустить"
            };
            _pauseResumeButton = new()
            {
                Icon = Icons.Pause,
                ToolTip = "Пауза"
            };
            _recompileButton = new()
            {
                Icon = Icons.Update,
                ToolTip = "Собрать заново"
            };

            UpdateName();

            dialog.PropertyChanged += OnDialogPropertyChanged;
            _startStopButton.Clicked += OnStartStopButtonClicked;
            _pauseResumeButton.Clicked += OnPauseResumeButtonClicked;
            _recompileButton.Clicked += OnRecompileButtonClicked;
        }

        public event EventHandler? CloseRequested;

        public ProjectDialog Dialog { get; }
        public string Name
        {
            get => field ?? string.Empty;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(Name));
                    field = value;
                    InvokePropertyChanged(nameof(Name));
                }
            }
        }
        public bool CanRename => false;
        public bool CanClose => true;
        public UIElement TabContent
        {
            get
            {
                if (IsDisposed)
                {
                    throw new InvalidOperationException("Невозможно получить представление для вкладки для очищенного объекта");
                }

                if (_view == null)
                {
                    _view = _compilerViewsPool.GetElement();
                    _view.Dialog = Dialog;
                }

                return _view;
            }
        }
        public IEnumerable<ActionButton>? Actions
        {
            get
            {
                field ??= [_startStopButton, _pauseResumeButton, _recompileButton];
                return field;
            }
        }

        private readonly ActionButton _startStopButton;
        private readonly ActionButton _pauseResumeButton;
        private readonly ActionButton _recompileButton;
        private DialogCompilerView? _view;

        #region Управление

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Dialog.PropertyChanged -= OnDialogPropertyChanged;
            _startStopButton.Clicked -= OnStartStopButtonClicked;
            _pauseResumeButton.Clicked -= OnPauseResumeButtonClicked;
            _recompileButton.Clicked -= OnRecompileButtonClicked;

            if (_view != null)
            {
                _view.Dialog = null;
                _compilerViewsPool.Free(_view);
                _view = null;
            }

            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateName()
        {
            Name = $"Проигрывание: {Dialog.Name}";
        }

        #endregion

        #region События

        public void OnClosed(object? sender, EventArgs e)
        {
        }
        public void OnHided(object? sender, EventArgs e)
        {
        }
        public void OnShowed(object? sender, EventArgs e)
        {
        }

        private void OnDialogPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Name))
            {
                UpdateName();
            }
        }

        private void OnRecompileButtonClicked(object? sender, object? e)
        {
        }
        private void OnPauseResumeButtonClicked(object? sender, object? e)
        {
        }
        private void OnStartStopButtonClicked(object? sender, object? e)
        {
            _view?.Compile();
        }

        #endregion

        #region Статика


        private static readonly ElementsPool<DialogCompilerView> _compilerViewsPool = new();

        #endregion
    }
}
