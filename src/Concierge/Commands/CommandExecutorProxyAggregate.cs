using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using Qoollo.Concierge.Commands.Executors;
using Qoollo.Concierge.UniversalExecution.AppModes;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.Commands
{
    internal class CommandExecutorProxyAggregate : CommandExecutorProxy
    {
        private readonly List<CommandExecutorWithRunMode> _addonCommands;
        private CommandExecutorProxy _commandExecutor;

        public CommandExecutorProxyAggregate(CommandExecutorProxy commandExecutor)
        {
            Contract.Requires(commandExecutor != null);
            UserCommandExecutor = commandExecutor;
            _commandExecutor = null;
            _addonCommands = new List<CommandExecutorWithRunMode>();
        }

        public CommandExecutorProxy UserCommandExecutor { get; private set; }

        public void SetCommandExecutorProxy(CommandExecutorProxy executorProxy, AppMode appMode)
        {
            foreach (string name in UserCommandExecutor.CommandsName.Where(executorProxy.IsCommandExist))
            {
                throw new DuplicateNameException(string.Format("Command {0} is already exist", name));
            }
            _commandExecutor = executorProxy;

            foreach (CommandExecutorWithRunMode addonCommand in _addonCommands.Where(x => x.IsEqualType(appMode)))
            {
                if (_commandExecutor.IsCommandExist(addonCommand.CommandName))
                    throw new DuplicateNameException(string.Format("Command {0} is already exist",
                        addonCommand.CommandName));

                addonCommand.SetMode(appMode);
                _commandExecutor.Add(addonCommand);
            }
        }

        public override string Execute(CommandSpec command)
        {
            string result = UserCommandExecutor.Execute(command);

            if (!result.Equals("Command not found"))
                return result;

            if (_commandExecutor != null)
                return _commandExecutor.Execute(command);

            return "Command not found";
        }

        public string GetHelp(string[] args)
        {
            string help = args.Length == 0 ? UserCommandExecutor.GetHelp() : UserCommandExecutor.GetHelp(args[0]);
            string helpExecutable = _commandExecutor == null
                ? ""
                : args.Length == 0 ? _commandExecutor.GetHelp() : _commandExecutor.GetHelp(args[0]);

            if (help == "Command not found" && helpExecutable == "Command not found")
                return "Command not found";

            if (helpExecutable == "Command not found")
                return help;

            if (help == "Command not found")
                return helpExecutable;

            return string.Format("{0}{1}", help, helpExecutable);
        }

        public void AddCommand(CommandExecutorWithRunMode command)
        {
            _addonCommands.Add(command);
        }
    }
}