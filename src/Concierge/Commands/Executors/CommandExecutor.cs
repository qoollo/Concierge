using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.Commands.Executors
{
    internal abstract class CommandExecutor
    {
        protected CommandExecutor(string commandName, string description)
        {
            CommandName = commandName;
            Description = description;
        }

        public string CommandName { get; private set; }
        public string Description { get; private set; }

        public abstract string Execute(CommandSpec command);
    }
}