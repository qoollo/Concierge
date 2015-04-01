using System;
using System.Linq;
using System.Net;
using Qoollo.Concierge.UniversalExecution.AppModes;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.Commands.Executors
{
    internal class CommandExecutorWithRunModeGeneric<TMode> : CommandExecutorWithRunMode
        where TMode : AppMode
    {
        private readonly Func<TMode, string, string[], string> _action;

        public CommandExecutorWithRunModeGeneric(string commandName, string description,
            Func<TMode, string, string[], string> action) : base(commandName, description, typeof (TMode))
        {
            _action = action;
        }

        public CommandExecutorWithRunModeGeneric(string commandName, string description,
            Action<TMode, string, string[]> action)
            : this(commandName, description, (mode, name, args) =>
            {
                action(mode, name, args);
                return HttpStatusCode.OK.ToString();
            })
        {
        }

        protected override string ExecuteInner(AppMode mode, CommandSpec command)
        {
            string concat = command.Arguments.Aggregate("", (current, kv) => current + (kv.Key + " " + kv.Value));
            return _action(mode as TMode, command.Name, concat.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}