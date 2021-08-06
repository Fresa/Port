using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Port.Server.Threading
{
    internal sealed class TransitioningLock
    {
        private int _state = Released;
        private const int Locked = 1;
        private const int Released = 0;
        private const int Transitioning = -1;

        internal bool TryStartLocking([NotNullWhen(true)] out ILockState? state)
        {
            var success =
                Interlocked.CompareExchange(ref _state, Transitioning, Released) ==
                Released;

            if (!success)
            {
                state = null;
                return false;
            }

            state = new LockState(this);
            return true;
        }

        internal bool TryStartReleasing([NotNullWhen(true)] out ILockState? state)
        {
            var success =
                Interlocked.CompareExchange(ref _state, Transitioning, Locked) ==
                Locked;

            if (!success)
            {
                state = null;
                return false;
            }

            state = new LockState(this);
            return true;
        }
        
        private class LockState : ILockState
        {
            private readonly TransitioningLock _lock;

            internal LockState(TransitioningLock @lock)
            {
                _lock = @lock;
            }

            public void Release()
            {
                Interlocked.Exchange(ref _lock._state, Released);
            }

            public void Lock()
            {
                Interlocked.Exchange(ref _lock._state, Locked);
            }
        }
        
    }

    internal interface ILockState
    {
        void Release();
        void Lock();
    }
}