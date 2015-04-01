using System.Diagnostics.Contracts;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.Commands.Routers
{
    internal class LocalRouter : CommandRouter
    {
        private readonly CommandExecutorProxy _commandManager;

        public LocalRouter(CommandExecutorProxy commandManager)
        {
            Contract.Requires(commandManager != null);
            _commandManager = commandManager;
        }

        public override string Send(CommandSpec command)
        {
            return _commandManager.Execute(command);
        }
    }
}