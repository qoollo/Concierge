using System;
using Qoollo.Concierge.UniversalExecution.AppModes;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.Commands.Executors
{
    internal abstract class CommandExecutorWithRunMode : CommandExecutor
    {
        private readonly Type _type;
        private AppMode _mode;

        protected CommandExecutorWithRunMode(string commandName, string description, Type type)
            : base(commandName, description)
        {
            _type = type;
        }

        public bool IsEqualType(AppMode mode)
        {
            return _type == mode.GetType();
        }

        public void SetMode(AppMode mode)
        {
            _mode = mode;
        }

        public override string Execute(CommandSpec command)
        {
            return ExecuteInner(_mode, command);
        }

        protected abstract string ExecuteInner(AppMode mode, CommandSpec command);
    }
}