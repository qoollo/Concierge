using System;
using System.Diagnostics.Contracts;
using System.Net;

namespace Qoollo.Concierge.Commands.Executors
{
    internal class CommandExecutorFromMethod<TCommand> : CommandExecutorWithClass<TCommand>
        where TCommand : UserCommand
    {
        private readonly Func<TCommand, string> _action;

        public CommandExecutorFromMethod(string commandName, string description, Func<TCommand, string> action)
            : base(commandName, description)
        {
            Contract.Requires(action != null);
            _action = action;
        }

        public CommandExecutorFromMethod(string commandName, string description, Action<TCommand> action)
            : this(commandName, description, command =>
            {
                action(command);
                return HttpStatusCode.OK.ToString();
            })
        {
        }

        protected override string ExecuteGeneric(TCommand command)
        {
            return _action(command);
        }
    }
}