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

        public CommandExecutorWithArgs(string commandName, string description,
            Func<string, string[], string> action)
            : base(commandName, description)
        {
            Contract.Requires(action != null);
            _action = action;
        }

        public CommandExecutorWithArgs(string commandName, string description,
            Action<string, string[]> action)
            : this(commandName, description, (name, args) =>
            {
                action(name, args);
                return HttpStatusCode.OK.ToString();
            })
        {
        }

        public override string Execute(CommandSpec command)
        {
            string concat = command.Arguments.Aggregate("", (current, kv) => current + (kv.Key + " " + kv.Value + " "));

            return _action(command.Name, concat.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}