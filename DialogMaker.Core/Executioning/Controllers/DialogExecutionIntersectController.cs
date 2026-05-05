namespace DialogMaker.Core.Executioning
{
    public class DialogExecutionIntersectController(IDialogExecutingThreadManager threadManager, IJoinOperationInfo info)
        : Disposable, IJoinController
    {
        public bool IsCompleted
        {
            get => field;
            private set
            {
                if (field != value)
                {
                    OnPropertyChanging(nameof(IsCompleted));
                    field = value;
                    OnPropertyChanged(nameof(IsCompleted));
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
                    OnPropertyChanging(nameof(IsBusy));
                    field = value;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }
        public IJoinOperationInfo JoinInfo { get; } = info;

        private readonly IDialogExecutingThreadManager _threadManager = threadManager;
        private readonly HashSet<IDialogExecutionThread> _joinedThreads = [];

        #region Управление

        public bool CanJoin(DialogExecutionContext context)
        {
            return DialogExecutionJoinController.CanJoin(context, JoinInfo, _joinedThreads, IsBusy);
        }

        public async Task<bool> Join(DialogExecutionContext context)
        {
            if (IsCompleted)
            {
                throw new DialogExecutionException($"Невозможно использовать контроллер пересечения потоков, который уже завершил работу. Его надо сначала очистить");
            }
            if (IsDisposed ||
                JoinInfo.InputSections.Count == 0 ||
                JoinInfo.Outputs.Count == 0)
            {
                return false;
            }

            IsBusy = true;
            int currentSection = context.CurrentThread.PreviousSection;

            if (!JoinInfo.InputSections.Contains(currentSection))
            {
                throw new DialogExecutionException($"Вошёл неожиданный поток с неожиданным сегментом: {currentSection}");
            }

            _joinedThreads.Add(context.CurrentThread);

            if (_joinedThreads.Count == JoinInfo.InputSections.Count)
            {
                IsCompleted = true;
                IsBusy = false;
            }

            return _joinedThreads.Count == 1;
        }
        public void Clear()
        {
            if (IsDisposed)
            {
                return;
            }

            _joinedThreads.Clear();
            IsCompleted = false;
            IsBusy = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            _joinedThreads.Clear();
        }

        #endregion
    }
}
