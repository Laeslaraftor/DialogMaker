using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace DialogMaker.Lib
{
    /// <summary>
    /// Синхронизатор списков
    /// </summary>
    public class CollectionSynchronizer2<T> : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Создать новый экземпляр синхронизатора коллекций
        /// </summary>
        /// <param name="First"><inheritdoc cref="_FirstCollection"/></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CollectionSynchronizer2(INotifyCollectionChanged First) : this(First, new ObservableCollection<T>((IEnumerable<T>)First))
        {
        }
        /// <summary>
        /// Создать новый экземпляр синхронизатора коллекций
        /// </summary>
        /// <param name="First"><inheritdoc cref="_FirstCollection"/></param>
        /// <param name="Second"><inheritdoc cref="_SecondCollection"/></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CollectionSynchronizer2(INotifyCollectionChanged First, INotifyCollectionChanged Second)
        {
            FirstCollection = First ?? throw new ArgumentNullException(nameof(First));
            SecondCollection = Second ?? throw new ArgumentNullException(nameof(Second));

            if (First is not IList<T> Collection)
            {
                throw new ArgumentException($"Объект не является списком!", nameof(First));
            }
            if (Second is not IList<T> Collection2)
            {
                throw new ArgumentException($"Объект не является списком!", nameof(Second));
            }

            FirstCollection.CollectionChanged += FirstCollectionChanged;
            SecondCollection.CollectionChanged += SecondCollectionChanged;

            _FirstCollection = Collection;
            _SecondCollection = Collection2;
        }
        /// <summary>
        /// Очистка синхронизатора
        /// </summary>
#pragma warning disable CA1063 // Правильно реализуйте IDisposable
        ~CollectionSynchronizer2()
#pragma warning restore CA1063 // Правильно реализуйте IDisposable
        {
            Dispose(false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Первая отслеживаемая коллекция
        /// </summary>
        public INotifyCollectionChanged FirstCollection { get; }
        /// <summary>
        /// Вторая отслеживаемая коллекция
        /// </summary>
        public INotifyCollectionChanged SecondCollection { get; }
        /// <summary>
        /// Обновляются ли сейчас коллекции
        /// </summary>
        public bool IsUpdating
        {
            get => _IsUpdating;
            private set
            {
                if (_IsUpdating != value)
                {
                    _IsUpdating = value;
                    InvokePropertyChanged(nameof(IsUpdating));
                }
            }
        }

        private readonly IList<T> _FirstCollection;
        private readonly IList<T> _SecondCollection;
        private bool _IsUpdating;

        #region Управление

        private bool TrySync(IList<T> From, IList<T> To, NotifyCollectionChangedEventArgs Args)
        {
            if (IsUpdating)
            {
                return false;
            }

            IsUpdating = true;

            try
            {
                Sync(From, To, Args);
            }
            finally
            {
                IsUpdating = false;
            }

            return true;
        }
        private void SynchronizeFromFirstToSecond()
        {
            _IsUpdating = true;

            try
            {
                _SecondCollection.Clear();
                foreach (var item in _FirstCollection)
                {
                    _SecondCollection.Add(item);
                }
            }
            finally
            {
                _IsUpdating = false;
            }
        }

        #endregion

        #region События

        private void InvokePropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new(PropertyName));
        }
        private void FirstCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TrySync(_FirstCollection, _SecondCollection, e);
        }
        private async void SecondCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TrySync(_SecondCollection, _FirstCollection, e);
        }

        #endregion

        #region Статика

        private static void Sync(IList<T> From, IList<T> To, NotifyCollectionChangedEventArgs Args)
        {
            switch (Args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (Args.NewItems != null)
                    {
                        for (int i = 0; i < Args.NewItems.Count; i++)
                        {
                            var newIndex = Args.NewStartingIndex + i;
                            if (newIndex <= To.Count)
                            {
                                To.Insert(newIndex, (T)Args.NewItems[i]);
                            }
                            else
                            {
                                To.Add((T)Args.NewItems[i]);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (Args.OldStartingIndex >= 0 && Args.OldStartingIndex < To.Count)
                    {
                        for (int i = 0; i < Args.OldItems.Count; i++)
                        {
                            To.RemoveAt(Args.OldStartingIndex);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (Args.NewStartingIndex >= 0 && Args.NewStartingIndex < To.Count)
                    {
                        To[Args.NewStartingIndex] = (T)Args.NewItems[0];
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    if (Args.OldStartingIndex >= 0 && Args.OldStartingIndex < To.Count &&
                        Args.NewStartingIndex >= 0 && Args.NewStartingIndex <= To.Count)
                    {
                        var item = To[Args.OldStartingIndex];
                        To.RemoveAt(Args.OldStartingIndex);
                        To.Insert(Args.NewStartingIndex, item);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    To.Clear();
                    foreach (var item in From)
                    {
                        To.Add(item);
                    }
                    break;
            }
        }

        #endregion

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Очистка синхронизатора
        /// </summary>
        /// <param name="IsDisposing"></param>
        protected virtual void Dispose(bool IsDisposing)
        {
            FirstCollection.CollectionChanged -= FirstCollectionChanged;
            SecondCollection.CollectionChanged -= SecondCollectionChanged;
        }
    }
}
