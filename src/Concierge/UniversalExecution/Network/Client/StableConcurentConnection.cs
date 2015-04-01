using System;
using System.Diagnostics.Contracts;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;

namespace Qoollo.Concierge.UniversalExecution.Network.Client
{
    internal class ConcurrentConnection : IDisposable
    {
        private readonly IClientChannel _channel;
        private readonly SemaphoreSlim _concurrencyTrackSemaphore;
        private readonly int _maxAsyncQueryCount;
        private readonly TimeSpan _operationTimeout;
        private readonly string _targetName;
        private int _concurrenyRequestCount;

        private volatile bool _isDisposeRequested;
        private volatile bool _isDisposed;
        private int _state;

        public ConcurrentConnection(IClientChannel channel, int maxAsyncQueryCount)
        {
            Contract.Requires(channel != null);

            _channel = channel;
            _channel.Faulted += ChannelFaultedHandler;
            _channel.Closed += ChannelClosedExternallyHandler;
            _targetName = _channel.RemoteAddress.ToString();
            _operationTimeout = _channel.OperationTimeout;

            _concurrenyRequestCount = 0;
            _maxAsyncQueryCount = 0;
            if (maxAsyncQueryCount > 0)
            {
                _maxAsyncQueryCount = maxAsyncQueryCount;
                _concurrencyTrackSemaphore = new SemaphoreSlim(_maxAsyncQueryCount);
            }

            _state = (int) StableConnectionState.Created;
            _isDisposed = false;
            _isDisposeRequested = false;
        }


        public IClientChannel Channel
        {
            get { return _channel; }
        }

        public string TargetName
        {
            get { return _targetName; }
        }

        public StableConnectionState State
        {
            get { return (StableConnectionState) _state; }
        }

        public bool CanBeUsedForCommunication
        {
            get { return State == StableConnectionState.Opened; }
        }

        public int ConcurrentRequestCount
        {
            get { return _concurrenyRequestCount; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event EventHandler Closed;
        public event EventHandler Opened;


        private bool IsValidStateTransition(StableConnectionState oldState, StableConnectionState newState)
        {
            switch (oldState)
            {
                case StableConnectionState.Created:
                    return newState == StableConnectionState.Opening || newState == StableConnectionState.Closed;
                case StableConnectionState.Opening:
                    return newState == StableConnectionState.Opened || newState == StableConnectionState.Closed;
                case StableConnectionState.Opened:
                    return newState == StableConnectionState.Closed;
                case StableConnectionState.Closed:
                    return newState == StableConnectionState.Invalid;
                case StableConnectionState.Invalid:
                    return false;
                default:
                    throw new InvalidProgramException("Unknown StableConnectionState value: " + oldState);
            }
        }

        private StableConnectionState ChangeStateSafe(StableConnectionState newState)
        {
            int curState = _state;

            if (!IsValidStateTransition((StableConnectionState) curState, newState))
                return (StableConnectionState) curState;

            if (Interlocked.CompareExchange(ref _state, (int) newState, curState) == curState)
                return (StableConnectionState) curState;

            var sw = new SpinWait();
            while (Interlocked.CompareExchange(ref _state, (int) newState, curState) != curState)
            {
                sw.SpinOnce();
                curState = _state;
                if (!IsValidStateTransition((StableConnectionState) curState, newState))
                    return (StableConnectionState) curState;
            }

            return (StableConnectionState) curState;
        }


        private void OnClosed()
        {
            if (ChangeStateSafe(StableConnectionState.Closed) == StableConnectionState.Closed)
                return;

            EventHandler tmp = Closed;
            if (tmp != null)
                tmp(this, EventArgs.Empty);

            TryFreeAllResources();
        }

        private void OnOpened()
        {
            if (ChangeStateSafe(StableConnectionState.Opened) != StableConnectionState.Opening)
                return;

            EventHandler tmp = Opened;
            if (tmp != null)
                tmp(this, EventArgs.Empty);
        }


        private void ChannelFaultedHandler(object sender, EventArgs e)
        {
            OnClosed();
        }

        private void ChannelClosedExternallyHandler(object sender, EventArgs e)
        {
            OnClosed();
        }


        public bool Open(int timeout)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
            if (ChangeStateSafe(StableConnectionState.Opening) != StableConnectionState.Created)
                throw new InvalidOperationException("Can't open channel which is in state: " + _state);

            try
            {
                if (timeout < 0)
                    _channel.Open();
                else
                    _channel.Open(TimeSpan.FromMilliseconds(timeout));

                if (_channel.State == CommunicationState.Opened)
                    OnOpened();
                else
                    Close();
            }
            catch (CommunicationException cex)
            {
                OnClosed();
            }
            catch (TimeoutException tex)
            {
                OnClosed();
            }

            return State == StableConnectionState.Opened;
        }

        public bool Open()
        {
            return Open(-1);
        }


        public void OpenAsync()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
            if (ChangeStateSafe(StableConnectionState.Opening) != StableConnectionState.Created)
                throw new InvalidOperationException("Can't open channel, that is in state: " + _state);

            try
            {
                _channel.BeginOpen(OpenedAsyncHandler, null);
            }
            catch (CommunicationException cex)
            {
                OnClosed();
            }
            catch (TimeoutException tex)
            {
                OnClosed();
            }
        }

        private void OpenedAsyncHandler(IAsyncResult res)
        {
            try
            {
                _channel.EndOpen(res);
                if (_channel.State == CommunicationState.Opened)
                    OnOpened();
                else
                    Close();
            }
            catch (CommunicationException cex)
            {
                OnClosed();
            }
            catch (TimeoutException tex)
            {
                OnClosed();
            }
        }


        public ConcurrentRequestTracker RunRequest(int timeout)
        {
            if (_isDisposed)
                return ConcurrentRequestTracker.Empty;

            if (_concurrencyTrackSemaphore != null)
            {
                if (!_concurrencyTrackSemaphore.Wait(timeout))
                {
                    throw new TimeoutException(
                        "RunRequest failed due to timeout in waiting for concurrent resource. Current Timeout: " +
                        timeout + " ms, Target: " + _targetName);
                }
            }

            Interlocked.Increment(ref _concurrenyRequestCount);
            return new ConcurrentRequestTracker(this);
        }


        public void FinishRequest()
        {
            if (_isDisposed)
                return;

            if (_concurrencyTrackSemaphore != null)
                _concurrencyTrackSemaphore.Release();

            Interlocked.Decrement(ref _concurrenyRequestCount);

            TryFreeAllResources();
        }


        private void Close()
        {
            try
            {
                _channel.Faulted -= ChannelFaultedHandler;
                _channel.Closed -= ChannelClosedExternallyHandler;

                if (_channel.State == CommunicationState.Opened)
                    _channel.Close();
                else
                    _channel.Abort();
            }
            catch (Exception ex)
            {
                _channel.Abort();
            }
            finally
            {
                OnClosed();
            }
        }

        private void CloseFromFinalizer()
        {
            try
            {
                _channel.Faulted -= ChannelFaultedHandler;
                _channel.Closed -= ChannelClosedExternallyHandler;

                if (_channel != null)
                    _channel.Abort();
            }
            finally
            {
                ChangeStateSafe(StableConnectionState.Closed);
            }
        }


        private void FreeAllResources()
        {
            if (!_isDisposed)
            {
                _isDisposeRequested = true;
                _isDisposed = true;

                StableConnectionState prevState = ChangeStateSafe(StableConnectionState.Invalid);
                Contract.Assume(prevState == StableConnectionState.Closed);

                if (_concurrencyTrackSemaphore != null)
                    _concurrencyTrackSemaphore.Dispose();

                try
                {
                    if (_channel.State == CommunicationState.Faulted)
                        _channel.Abort();
                    _channel.Dispose();
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void TryFreeAllResources()
        {
            if (_isDisposeRequested && !_isDisposed && State == StableConnectionState.Closed &&
                (_concurrencyTrackSemaphore == null || _concurrencyTrackSemaphore.CurrentCount == _maxAsyncQueryCount))
            {
                FreeAllResources();
            }
        }


        protected virtual void Dispose(bool isUserCall)
        {
            _isDisposeRequested = true;

            if (isUserCall)
            {
                if (!_isDisposed)
                    Close();
            }
            else
            {
                if (!_isDisposed)
                {
                    CloseFromFinalizer();
                    FreeAllResources();
                }
            }
        }

        ~ConcurrentConnection()
        {
            Dispose(false);
        }
    }

    internal class StableConcurrentConnection<TApi> : IDisposable
    {
        private const int InitialOpenConnectionPause = 1000;
        private const int MaxOpenConnectionPause = 180000;

        private readonly ChannelFactory<TApi> _factory;
        private readonly int _maxAsyncQueryCount;
        private readonly ManualResetEventSlim _openWaiter;
        private ConcurrentConnection _channel;
        private volatile bool _isDisposed;
        private int _openConnectionPauseMs = InitialOpenConnectionPause;


        public StableConcurrentConnection(ChannelFactory<TApi> factory, int maxAsyncQueryCount,
            bool syncFirstOpen = false)
        {
            Contract.Requires(factory != null);

            _factory = factory;
            _maxAsyncQueryCount = maxAsyncQueryCount;
            _openWaiter = new ManualResetEventSlim();
            _isDisposed = false;

            ReinitChannel(syncFirstOpen);
        }

        public StableConnectionState State
        {
            get { return _channel.State; }
        }

        public bool CanBeUsedForCommunication
        {
            get { return _channel.CanBeUsedForCommunication; }
        }

        public int ConcurrentRequestCount
        {
            get { return _channel.ConcurrentRequestCount; }
        }

        public ServiceEndpoint EndPoint { get { return _factory.Endpoint; } }
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                ConcurrentConnection oldChannel = _channel;
                if (oldChannel != null)
                {
                    oldChannel.Closed -= ChannelClosedHandler;
                    oldChannel.Opened -= ChannelOpenedHandler;
                    oldChannel.Dispose();
                }
                _openWaiter.Dispose();
            }
        }

        protected virtual void OnConnectionRecreated()
        {
        }

        protected virtual void OnConnectionOpened()
        {
        }

        protected virtual void OnConnectionClosed()
        {
        }

        private void ReinitChannel(bool isSync = false)
        {
            ConcurrentConnection oldChannel = _channel;
            if (oldChannel != null)
            {
                oldChannel.Closed -= ChannelClosedHandler;
                oldChannel.Opened -= ChannelOpenedHandler;
            }


            var newChannel = new ConcurrentConnection((IClientChannel) _factory.CreateChannel(), _maxAsyncQueryCount);

            if (Interlocked.CompareExchange(ref _channel, newChannel, oldChannel) != oldChannel || _isDisposed)
            {
                newChannel.Dispose();
            }
            else
            {
                _openWaiter.Reset();
                OnConnectionRecreated();
                newChannel.Closed += ChannelClosedHandler;
                newChannel.Opened += ChannelOpenedHandler;
                if (isSync)
                    newChannel.Open();
                else
                    newChannel.OpenAsync();
            }

            if (oldChannel != null)
                oldChannel.Dispose();
        }

        private void ChannelClosedHandler(object sender, EventArgs e)
        {
            OnConnectionClosed();

            if (!_isDisposed)
            {
                _openWaiter.Reset();

                _openConnectionPauseMs = 2*_openConnectionPauseMs;
                if (_openConnectionPauseMs > MaxOpenConnectionPause)
                    _openConnectionPauseMs = MaxOpenConnectionPause;


                Thread.Sleep(_openConnectionPauseMs);
                ReinitChannel();

            }
        }

        private void ChannelOpenedHandler(object sender, EventArgs e)
        {
            _openConnectionPauseMs = InitialOpenConnectionPause;
            OnConnectionOpened();
            _openWaiter.Set();
        }


        public ConcurrentRequestTracker<TApi> RunRequest(int timeout)
        {
            return new ConcurrentRequestTracker<TApi>(_channel.RunRequest(timeout));
        }
    }
}