using Acly.Serialize;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class DialogThread : Disposable, IDialogExecutionThread
    {
        internal DialogThread(DialogExecutor executor, IDialogExecutingHandler handler, DialogByteCodeData data)
        {
            DialogExecutor = executor;
            _handler = handler;
            _data = data;
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
        public int CurrentOperation
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(CurrentOperation));
                    field = value;
                    InvokePropertyChanged(nameof(CurrentOperation));
                }
            }
        }

        private readonly IDialogExecutingHandler _handler;
        private readonly DialogByteCodeData _data;
        private CancellationTokenRegistration? _currentCancellationTokenRegistration;

        #region Управление

        public async Task Start(int sectionId)
        {
            if (IsRunning)
            {
                return;
            }

            JumpTo(sectionId);

            if (0 >= _data.Sections[CurrentSection].Operations.Count)
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
            DialogExecutionContext context = new(this, DialogExecutor.Resources, _handler, token);

            while (CurrentOperation < _data.Sections[CurrentSection].Operations.Count)
            {
                int operationIndex = CurrentOperation;
                int sectionIndex = CurrentSection;

                var operation = _data.Sections[sectionIndex].Operations[operationIndex];
                var opCode = Operation.GetImplementation(operation.Code);

                await opCode.Execute(context, operation.Arguments);

                if (token.IsCancellationRequested)
                {
                    break;
                }
                if (sectionIndex == CurrentSection && operationIndex == CurrentOperation)
                {
                    CurrentOperation++;
                }
            }
        }

        public void JumpTo(int sectionId)
        {
            if (CurrentSection != sectionId)
            {
                CurrentSection = sectionId;
            }

            Goto(0);
        }
        public void Goto(int instructionPosition)
        {
            CurrentOperation = instructionPosition;
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
            IsRunning = false;
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
        }

        #endregion
    }
}
