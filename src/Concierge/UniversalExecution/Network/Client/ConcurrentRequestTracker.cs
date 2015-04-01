using System;
using System.Diagnostics.Contracts;
using System.ServiceModel;
using System.Threading;

namespace Qoollo.Concierge.UniversalExecution.Network.Client
{
    internal enum StableConnectionState
    {
        Created,
        Opening,
        Opened,
        Closed,
        Invalid
    }

    internal class ConcurrentRequestTracker : IDisposable
    {
        private static readonly ConcurrentRequestTracker _empty = new ConcurrentRequestTracker();

        // ==========

        private ConcurrentConnection _connectionSource;

        protected ConcurrentRequestTracker()
        {
            _connectionSource = null;
        }

        public ConcurrentRequestTracker(ConcurrentConnection conSrc)
        {
            Contract.Requires(conSrc != null);

            _connectionSource = conSrc;
        }

        public static ConcurrentRequestTracker Empty
        {
            get { return _empty; }
        }

        public IClientChannel Channel
        {
            get { return _connectionSource.Channel; }
        }

        public StableConnectionState State
        {
            get
            {
                ConcurrentConnection tmp = _connectionSource;
                if (tmp != null)
                    return tmp.State;
                return StableConnectionState.Invalid;
            }
        }

        public bool CanBeUsedForCommunication
        {
            get { return State == StableConnectionState.Opened; }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isUserCall)
        {
            ConcurrentConnection tmp = Interlocked.Exchange(ref _connectionSource, null);
            if (tmp != null)
                tmp.FinishRequest();
        }

        ~ConcurrentRequestTracker()
        {
            Dispose(false);
        }
    }

    internal struct ConcurrentRequestTracker<TApi> : IDisposable
    {
        private readonly ConcurrentRequestTracker _inner;

        public ConcurrentRequestTracker(ConcurrentRequestTracker innerTracker)
        {
            Contract.Requires(innerTracker != null);

            _inner = innerTracker;
        }

        public TApi API
        {
            get { return (TApi) _inner.Channel; }
        }

        public bool CanBeUsedForCommunication
        {
            get { return _inner.CanBeUsedForCommunication; }
        }

        public void Dispose()
        {
            _inner.Dispose();
        }
    }
}