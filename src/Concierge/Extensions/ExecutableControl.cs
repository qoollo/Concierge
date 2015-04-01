using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Qoollo.Concierge.Logger;

namespace Qoollo.Concierge.Extensions
{
    internal class ExecutableControl : IExecutable
    {
        private readonly AutoResetEvent _event;
        private readonly IExecutable _executable;
        private readonly IConciergeLogger _logger;
        private bool _isStart;

        public ExecutableControl(IExecutable executable, IConciergeLogger logger)
        {
            Contract.Requires(executable != null);
            _executable = executable;
            _logger = logger;
            _event = new AutoResetEvent(false);
        }

        public void Dispose()
        {
            _executable.Dispose();
        }

        public void Start()
        {
            CommandStart();
            _event.WaitOne();
        }

        public void Stop()
        {
            CommandStop();
            _event.Set();
        }

        public void CommandRestart()
        {
            CommandStop();
            CommandStart();
        }

        public void CommandStart()
        {
            if (!_isStart)
            {
                _isStart = true;
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        _executable.Start();
                    }
                    catch (Exception e)
                    {
                        _logger.Log(e.ToString());
                        throw;
                    }                    
                });
            }
        }

        public void CommandStop()
        {
            if (_isStart)
            {
                _isStart = false;
                _executable.Stop();
            }
        }
    }
}