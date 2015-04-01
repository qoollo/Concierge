using System;
using System.Diagnostics.Contracts;
using Qoollo.Concierge.Commands.Sources;

namespace Qoollo.Concierge.UniversalExecution.Decorators
{
    /// <summary>
    /// IExecutable decorator with command source
    /// </summary>
    internal class CommandSourceDecorator : IExecutable, IDecoratorChain
    {
        private readonly CommandSource _commandSource;
        private readonly IExecutable _executable;

        public CommandSourceDecorator(IExecutable executable, CommandSource commandSource)
        {
            Contract.Requires(commandSource != null);
            _executable = executable;
            _commandSource = commandSource;
        }

        public IExecutable FindDecorator(Type type)
        {
            if (_executable.GetType() == type)
                return _executable;

            var search = _executable as IDecoratorChain;
            if (search != null) return search.FindDecorator(type);
            return null;
        }

        public void Start()
        {
            if (_executable != null)
                _executable.Start();
            _commandSource.StartListening();
        }

        public void Stop()
        {
            _commandSource.StopListening();
            if (_executable != null)
                _executable.Stop();
        }

        public void Dispose()
        {
            _commandSource.StopListening();
            if (_executable != null)
                _executable.Dispose();
        }

        public CommandSource GetSource()
        {
            return _commandSource;
        }
    }
}