using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.Commands.Executors
{
    internal class CommandExecutorWithDictionary : CommandExecutor
    {
        private readonly Func<string, Dictionary<string, string>, string> _action;

        public CommandExecutorWithDictionary(string commandName, string description,
            Func<string, Dictionary<string, string>, string> action)
            : base(commandName, description)
        {
            Contract.Requires(action != null);
            _action = action;
        }

        public CommandExecutorWithDictionary(string commandName, string description,
            Action<string, Dictionary<string, string>> action)
            : this(commandName, description, (name, args) =>
            {
                action(name, args);
                return HttpStatusCode.OK.ToString();
            })
        {
        }

        public override string Execute(CommandSpec command)
        {
            return _action(command.Name, command.Arguments);
        }
    }
}