using System;

namespace DialogMaker.Core
{
    public interface IThreadDispatcher
    {
        public bool CurrentThreadIsMain { get; }

        public void Execute(Action action);
    }
}
