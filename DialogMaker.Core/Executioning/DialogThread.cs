using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class DialogThread(IDialogExecutingThreadManager threadManager, Stack<OperandValue> stack, IDialogExecutionResources resources, IDialogExecutingHandler handler, DialogByteCodeData data) 
        : Disposable, IDialogExecutionThread
    {
        public IDialogExecutingThreadManager ThreadManager { get; } = threadManager;
        public bool IsPaused
        {
            get => field;
            set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(IsPaused));
                    field = value;
                    InvokePropertyChanged(nameof(IsPaused));
                }
            }
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
        public int PreviousSection
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(PreviousSection));
                    field = value;
                    InvokePropertyChanged(nameof(PreviousSection));
                }
            }
        }
        public int CurrentSection
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(CurrentSection));
                    PreviousSection = field;
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

        private readonly IDialogExecutionResources _resources = resources;
        private readonly IDialogExecutingHandler _handler = handler;
        private readonly Stack<OperandValue> _stack = stack;
        private readonly DialogByteCodeData _data =data;
        private CancellationTokenSource? _currentCancellationTokenSource;

        #region Управление

        public async Task Start(int sectionId, int instructionPosition, IDialogExecutionThread? source = null)
        {
            if (IsRunning)
            {
                return;
            }

            JumpTo(sectionId);
            Goto(instructionPosition);

            if (source != null)
            {
                PreviousSection = source.CurrentSection;
            }

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
        public void StartThread(DialogPosition position)
        {
            ThreadManager.StartThread(this, position);
        }
        public void Stop()
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

        public void Push(OperandValue value)
        {
            _stack.Push(value);
        }
        public OperandValue Pop()
        {
            if (_stack.TryPop(out var value))
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
                    Stop();
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                }
            }
        }

        private async Task Start(CancellationToken token)
        {
            DialogExecutionContext context = new(this, ThreadManager, _resources, _handler, token);

            while (CurrentOperation < _data.Sections[CurrentSection].Operations.Count)
            {
                int operationIndex = CurrentOperation;
                int sectionIndex = CurrentSection;

                var operation = _data.Sections[sectionIndex].Operations[operationIndex];
                var opCode = Operation.GetImplementation(operation.Code);

                await WaitPause(token);
                await opCode.Execute(context, operation.Arguments);
                await WaitPause(token);

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
        private async Task WaitPause(CancellationToken cancellationToken)
        {
            while (IsPaused && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(50);
            }
        }

        #endregion
    }
}
