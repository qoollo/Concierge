using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.Commands.Executors
{
    internal class CommandExecutorWithArgs : CommandExecutor
    {
        private readonly Func<string, string[], string> _action;
        private readonly string _smartHelp;

        public CommandExecutorWithArgs(string commandName, string description,
            Func<string, string[], string> action, string smartHelp = "")
            : base(commandName, description)
        {
            Contract.Requires(action != null);
            _action = action;
            _smartHelp = smartHelp;
        }

        public CommandExecutorWithArgs(string commandName, string description,
            Action<string, string[]> action, string smartHelp = "")
            : this(commandName, description, (name, args) =>
            {
                action(name, args);
                return HttpStatusCode.OK.ToString();
            }, smartHelp)
        {
        }

        public override string Execute(CommandSpec command)
        {
            string concat = command.Arguments.Aggregate("", (current, kv) => current + (kv.Key + " " + kv.Value + " "));

            return _action(command.Name, concat.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
        }

        public override string GetHelp()
        {
            return _smartHelp;
        }
    }
}