using System.Diagnostics.Contracts;
using Qoollo.Concierge.Attributes;

namespace Qoollo.Concierge.UniversalExecution.Decorators
{
    internal class ActionsExecutableDecorator : IExecutable
    {
        private readonly FluentExecutor _executor;

        public ActionsExecutableDecorator(FluentExecutor executor)
        {
            Contract.Requires(executor != null);
            _executor = executor;
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            _executor.Start();
        }

        public void Stop()
        {
            if (_executor.Stop != null)
                _executor.Stop();
        }

        [StartupParametersHandler]
        public void StartupMethod(string[] keys)
        {
            if (_executor.Init != null)
                _executor.Init(keys);
        }
    }
}