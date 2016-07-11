using System;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge.UniversalExecution.Decorators
{
    /// <summary>
    /// IExecutable decorator for service starting
    /// </summary>
    internal class ServiceRunDecorator : IExecutable, IDecoratorChain
    {
        private readonly IExecutable _executable;

        public ServiceRunDecorator(IExecutable executable)
        {
            _executable = executable;
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
            var executor = new InstallerBase();
            executor.RunAsWinService(_executable);
        }

        public void Stop()
        {
            _executable.Stop();
        }
    }
}