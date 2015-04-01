using System;

namespace Qoollo.Concierge.UniversalExecution.Decorators
{
    /// <summary>
    /// IExecutable decorator for custom decorator search
    /// </summary>
    internal class TunnelDecorator : IExecutable, IDecoratorChain
    {
        public TunnelDecorator(IExecutable executable)
        {
            Executable = executable;
        }

        public IExecutable Executable { get; private set; }

        public IExecutable FindDecorator(Type type)
        {
            if (Executable.GetType() == type)
                return Executable;

            var search = Executable as IDecoratorChain;
            if (search != null) return search.FindDecorator(type);
            return null;
        }

        public void Dispose()
        {
            Executable.Dispose();
        }

        public void Start()
        {
            Executable.Start();
        }

        public void Stop()
        {
            Executable.Stop();
        }

        public void SetExecutable(IExecutable executable)
        {
            Executable = executable;
        }
    }
}