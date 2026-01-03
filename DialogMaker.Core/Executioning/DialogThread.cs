using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class DialogThread : Disposable, IDialogExecutionThread
    {
        internal DialogThread(DialogExecutor executor, byte[] code)
        {
            DialogExecutor = executor;
            _code = new MemoryStream(code);
        }

        public bool IsRunning
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(IsRunning));
                    field = value;
                    InvokePropertyChanged(nameof(IsRunning));   
                }
            }
        }
        public DialogExecutor DialogExecutor { get; }
        public int CurrentSection
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(CurrentSection));
                    field = value;
                    InvokePropertyChanged(nameof(CurrentSection));
                }
            }
        }
        public int CurrentSectionPosition
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(CurrentSectionPosition));
                    field = value;
                    InvokePropertyChanged(nameof(CurrentSectionPosition));
                }
            }
        }
        public int CurrentSectionLength
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(CurrentSectionLength));
                    field = value;
                    InvokePropertyChanged(nameof(CurrentSectionLength));
                }
            }
        }

        private readonly Stream _code;
        private CancellationTokenRegistration? _currentCancellationTokenRegistration;

        #region Управление

        public async Task Start(int sectionId)
        {
            if (IsRunning)
            {
                return;
            }

            JumpTo(sectionId);

            if (0 >= CurrentSectionLength)
            {
                return;
            }

            CancellationTokenRegistration cancellationTokenRegistration = new();
            cancellationTokenRegistration.Token.Register(() =>
            {
                IsRunning = false;
            });
            _currentCancellationTokenRegistration = cancellationTokenRegistration;
            IsRunning = true;

            await Start(cancellationTokenRegistration.Token);
            await cancellationTokenRegistration.DisposeAsync();

            IsRunning = false;
            _currentCancellationTokenRegistration = null;
        }
        private async Task Start(CancellationToken token)
        {
            DialogExecutionContext context = new(this, DialogExecutor.Resources, DialogExecutor.Handler, token);
            var codeStream = _code;

            while (IsRunning &&
                   _code.Position >= CurrentSectionPosition &&
                   _code.Position < CurrentSectionPosition + CurrentSectionLength)
            {
                Operation operation = Operation.Read(codeStream);
                var opCode = Operation.GetImplementation(operation.Code);

                await opCode.Execute(context, operation.Arguments);
            }
        }

        public void JumpTo(int sectionId)
        {
            if (CurrentSection != sectionId)
            {
                CurrentSection = sectionId;
                CurrentSectionPosition = DialogExecutor.Sections[sectionId];

                if (DialogExecutor.Sections.TryGetValue(sectionId + 1, out var nextPosition))
                {
                    CurrentSectionLength = nextPosition - CurrentSectionPosition;
                }
                else
                {
                    CurrentSectionLength = (int)_code.Length - CurrentSectionPosition;
                }
            }

            Goto(0);
        }
        public void Goto(int instructionPosition)
        {
            _code.Position = CurrentSectionPosition + instructionPosition;
        }
        public async void StartThread(int sectionId)
        {
            await DialogExecutor.StartThread(sectionId);
        }
        public void StopThread()
        {
            if (!IsRunning)
            {
                return;
            }
            if (_currentCancellationTokenRegistration == null)
            {
                throw new InvalidOperationException("Регистрационный узел токенов отмены отсутствует");
            }

            _currentCancellationTokenRegistration.Value.Dispose();
            _currentCancellationTokenRegistration = null;
        }
        public void StopExecuting()
        {
            DialogExecutor.Stop();
        }

        public void Push(OperandValue value)
        {
            DialogExecutor.Stack.Push(value);
        }
        public OperandValue Pop()
        {
            if (DialogExecutor.Stack.TryPop(out var value))
            {
                return value;
            }

            return false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (IsRunning)
            {
                try
                {
                    StopThread();
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }
            }

            _code.Dispose();
        }

        #endregion
    }
}
