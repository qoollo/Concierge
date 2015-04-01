using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.Commands.Routers
{
    internal abstract class CommandRouter
    {
        public abstract string Send(CommandSpec command);
    }
}