using System;
using System.Collections.Generic;
using DialogMaker.Core.Editor;
using System.Threading;
using System.Threading.Tasks;

namespace DialogMaker.Core.Executioning
{
    public class DialogExecutionJoinController(IDialogExecutingThreadManager threadManager, IJoinOperationInfo info)
        : Disposable, IJoinController
    {
        public bool IsCompleted
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(IsCompleted));
                    field = value;
                    InvokePropertyChanged(nameof(IsCompleted));
                }
            }
        }
        public bool IsBusy
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    InvokePropertyChanging(nameof(IsBusy));
                    field = value;
                    InvokePropertyChanged(nameof(IsBusy));
                }
            }
        }
        public IJoinOperationInfo JoinInfo { get; } = info;

        private readonly IDialogExecutingThreadManager _threadManager = threadManager;
        private readonly HashSet<IDialogExecutionThread> _joinedThreads = [];
        private CancellationTokenSource? _currentCancellationTokenSource;

        #region Управление

        public bool CanJoin(DialogExecutionContext context)
        {
            return CanJoin(context, JoinInfo, _joinedThreads, IsBusy);
        }

        public async Task<bool> Join(DialogExecutionContext context)
        {
            if (IsCompleted)
            {
                throw new DialogExecutionException($"Невозможно использовать контроллер объединения потоков, который уже завершил работу. Его надо сначала очистить");
            }
            if (IsDisposed ||
                JoinInfo.InputSections.Count == 0 ||
                JoinInfo.Outputs.Count == 0)
            {
                return false;
            }

            IsBusy = true;
            CancellationToken localCancellationToken;

            if (_currentCancellationTokenSource == null)
            {
                CancellationTokenSource source = new();
                localCancellationToken = source.Token;
                _currentCancellationTokenSource = source;
            }
            else
            {
                localCancellationToken = _currentCancellationTokenSource.Token;
            }

            int currentSection = context.CurrentThread.PreviousSection;

            if (!JoinInfo.InputSections.Contains(currentSection))
            {
                throw new DialogExecutionException($"Вошёл неожиданный поток с неожиданным сегментом: {currentSection}");
            }

            _joinedThreads.Add(context.CurrentThread);

            if (_joinedThreads.Count > 1)
            {
                return false;
            }

            while ((!AllThreadsJoined() || _threadManager.IsPaused) &&
                   !context.CancellationToken.IsCancellationRequested &&
                   !localCancellationToken.IsCancellationRequested &&
                   !IsDisposed)
            {
                await Task.Delay(50);
            }

            if (IsDisposed ||
                context.CancellationToken.IsCancellationRequested ||
                localCancellationToken.IsCancellationRequested)
            {
                return false;
            }

            IsCompleted = true;
            IsBusy = false;

            return true;
        }
        public void Clear()
        {
            if (IsDisposed)
            {
                return;
            }

            var currentTokenSource = _currentCancellationTokenSource;

            if (currentTokenSource != null)
            {
                try
                {
                    currentTokenSource.Cancel();
                    currentTokenSource.Dispose();
                }
                catch (Exception error)
                {
                    Logger.Log(error);
                }

                if (_currentCancellationTokenSource == currentTokenSource)
                {
                    _currentCancellationTokenSource = null;
                }
            }

            _joinedThreads.Clear();
            IsCompleted = false;
            IsBusy = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            try
            {
                _currentCancellationTokenSource?.Cancel();
                _currentCancellationTokenSource?.Dispose();
            }
            catch (Exception error)
            {
                Logger.Log(error);
            }

            _joinedThreads.Clear();
        }

        private bool AllThreadsJoined()
        {
            if (_joinedThreads.Count == JoinInfo.InputSections.Count)
            {
                foreach (var section in JoinInfo.InputSections)
                {
                    if (!SectionJoined(section))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
        private bool SectionJoined(int section)
        {
            foreach (var thread in _joinedThreads)
            {
                if (thread.PreviousSection == section)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Статика

        internal static bool CanJoin(DialogExecutionContext context, IJoinOperationInfo info, IEnumerable<IDialogExecutionThread> joinedThreads, bool isBusy)
        {
            if (info.InputSections.Count == 0)
            {
                return false;
            }
            if (isBusy)
            {
                foreach (var joinedThread in joinedThreads)
                {
                    if (joinedThread.PreviousSection == context.CurrentThread.PreviousSection)
                    {
                        return false;
                    }
                }
            }
            foreach (var input in info.InputSections)
            {
                if (input == context.CurrentThread.PreviousSection)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
