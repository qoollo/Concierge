using System;
using System.Threading;
using Qoollo.Concierge.Logger;

namespace Qoollo.Concierge.UniversalExecution.Decorators
{
    /// <summary>
    /// IExecutable decorator for async start
    /// </summary>
    internal class AsyncDecorator : IExecutable, IDecoratorChain
    {
        private readonly IExecutable _executable;
        private readonly IConciergeLogger _logger;
        private Thread _startThread;

        public AsyncDecorator(IExecutable executable, IConciergeLogger logger)
        {
            _executable = executable;
            _logger = logger;
        }

        public IExecutable FindDecorator(Type type)
        {
            if (_executable.GetType() == type)
                return _executable;

            var search = _executable as IDecoratorChain;
            if (search != null) return search.FindDecorator(type);
            return null;
        }

        public void Dispose()
        {
            _executable.Dispose();
        }

        public void Start()
        {
            _startThread = new Thread(InnerStart);
            _startThread.Start();
        }

        public void Stop()
        {
            _executable.Stop();
            _startThread.Join();
        }

        private void InnerStart()
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
        }
    }
}