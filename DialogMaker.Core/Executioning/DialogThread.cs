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
        private CancellationTokenSource? _currentCancellationTokenSource;

        #region Управление

        public async Task Start(int sectionId, int instructionPosition)
        {
            if (IsRunning)
            {
                return;
            }

            JumpTo(sectionId);
            Goto(instructionPosition);

            if (0 >= _data.Sections[CurrentSection].Operations.Count)
            {
                return;
            }

            CancellationTokenSource cancellationTokenSource = new();
            var cancelRegistration = cancellationTokenSource.Token.Register(() =>
            {
                IsRunning = false;
            });
            _currentCancellationTokenSource = cancellationTokenSource;
            IsRunning = true;

            await Start(cancellationTokenSource.Token);

            if (_currentCancellationTokenSource == cancellationTokenSource)
            {
                cancellationTokenSource.Cancel();
                await cancelRegistration.DisposeAsync();
                cancellationTokenSource.Dispose();
            }

            IsRunning = false;
            _currentCancellationTokenSource = null;
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
        public async void StartThread(int sectionId, int instructionPosition)
        {
            await DialogExecutor.StartThread(sectionId, instructionPosition);
        }
        public void StopThread()
        {
            if (!IsRunning)
            {
                return;
            }
            if (_currentCancellationTokenSource == null)
            {
                throw new InvalidOperationException("Источник токенов отмены отсутствует");
            }

            _currentCancellationTokenSource.Cancel();
            _currentCancellationTokenSource.Dispose();
            _currentCancellationTokenSource = null;
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
