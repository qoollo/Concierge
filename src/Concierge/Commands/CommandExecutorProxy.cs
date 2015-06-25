using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Qoollo.Concierge.Attributes;
using Qoollo.Concierge.Commands.Executors;
using Qoollo.Concierge.UniversalExecution.AppModes;
using Qoollo.Concierge.UniversalExecution.Network;
using Qoollo.Concierge.Whale;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge.Commands
{
    internal class CommandExecutorProxy
    {
        private readonly Dictionary<string, CommandExecutor> _commands = new Dictionary<string, CommandExecutor>();

        internal IEnumerable<CommandExecutor> Commands
        {
            get { return _commands.Values; }
        }

        #region Add commands

        public IEnumerable<string> CommandsName
        {
            get { return _commands.Keys; }
        }

        public void Add(CommandExecutor command)
        {
            if (_commands.Keys.Contains(command.CommandName))
                throw new DuplicateNameException(string.Format("Command {0} already exist", command.CommandName));

            _commands.Add(command.CommandName, command);
        }

        public void AddFromInstance(object instance)
        {
            // Get all commands from instance
            MethodInfo[] methods = instance.GetType().GetMethods();
            foreach (MethodInfo methodInfo in methods)
            {
                // get all commands with attribute
                if (Attribute.IsDefined(methodInfo, typeof (CommandHandlerAttribute)))
                {
                    var attr = methodInfo.GetCustomAttributes(typeof (CommandHandlerAttribute), false)
                        .First() as CommandHandlerAttribute;

                    // if it base command
                    if (CommandReflector.CheckFirstParameterType<UserCommand>(methodInfo))
                    {
                        var command = new CommandExecutorFromAttribute<UserCommand>(attr.Name, attr.Description,
                            instance, methodInfo);
                        Add(command);
                        continue;
                    }

                    Type commandType = CommandReflector.GetCommandTypeFromMethod(methodInfo);

                    if (commandType == null)
                        throw new InvalidDataException(WinServiceMessages.MethodParametersAreWrong(methodInfo.Name));

                    // Create command executor
                    CommandExecutor comm = CommandReflector.CreateExecutor(commandType, attr.Name, attr.Description,
                        instance, methodInfo);
                    Add(comm);
                }
            }
        }

        public void AddRange(IDictionary<string, CommandExecutor> commands)
        {
            foreach (var command in commands)
            {
                Add(command.Value);
            }
        }

        public void AddRange(IEnumerable<CommandExecutor> commands)
        {
            foreach (CommandExecutor command in commands)
            {
                Add(command);
            }
        }

        public void AddOrChangeRange(IEnumerable<CommandExecutor> commands)
        {
            foreach (CommandExecutor command in commands)
            {
                AddOrChangeCommand(command);
            }
        }

        public bool IsCommandExist(string name)
        {
            CommandExecutor command;
            return _commands.TryGetValue(name, out command);
        }

        public void AddOrChangeCommand(CommandExecutor command)
        {
            CommandExecutor comm;
            if (_commands.TryGetValue(command.CommandName, out comm))
                _commands[command.CommandName] = command;
            else
                _commands.Add(command.CommandName, command);
        }

        public string GetHelp()
        {
            IEnumerable<string> list = _commands.Select(command => command.Key + " " + command.Value.Description);
            return CustomConsoleHelpers.FormatHelp(list);
        }

        public string GetHelp(string name)
        {
            if (IsCommandExist(name))
                return _commands[name].GetHelp();
            return "Command not found";
        }

        #endregion

        #region BuildCommand

        public static CommandExecutor Build(string name, Action action, string helpText = "")
        {
            return new CommandExecutorWithDictionary(name, helpText, (n, dict) => action());
        }

        public static CommandExecutor Build(string name, Func<string> action, string helpText = "")
        {
            return new CommandExecutorWithDictionary(name, helpText, (n, dict) => action());
        }

        public static CommandExecutor Build(string name, Action<UserCommand> action, string helpText = "")
        {
            return new CommandExecutorFromMethod<UserCommand>(name, helpText, action);
        }

        public static CommandExecutor Build(string name, Func<UserCommand, string> action, string helpText = "")
        {
            return new CommandExecutorFromMethod<UserCommand>(name, helpText, action);
        }

        public static CommandExecutor Build(string name, Action<Dictionary<string, string>> action, string helpText = "")
        {
            return new CommandExecutorWithDictionary(name, helpText, (n, dict) => action(dict));
        }

        public static CommandExecutor Build(string name, Func<Dictionary<string, string>, string> action,
            string helpText = "")
        {
            return new CommandExecutorWithDictionary(name, helpText, (n, dict) => action(dict));
        }

        public static CommandExecutor Build(string name, Action<string[]> action, string helpText = "")
        {
            return new CommandExecutorWithArgs(name, helpText, (n, dict) => action(dict));
        }

        public static CommandExecutor Build(string name, Func<string[], string> action, string helpText = "", 
            string smartHelp = "")
        {
            return new CommandExecutorWithArgs(name, helpText, (n, dict) => action(dict), smartHelp);
        }

        public static CommandExecutor Build<T>(string name, Action<T> action, string helpText = "")
            where T : UserCommand
        {
            return new CommandExecutorFromMethod<T>(name, helpText, action);
        }

        public static CommandExecutor Build<T>(string name, Func<T, string> action, string helpText = "")
            where T : UserCommand
        {
            return new CommandExecutorFromMethod<T>(name, helpText, action);
        }

        public static CommandExecutorWithRunMode Build<TMode>(string name, Func<TMode, string, string[], string> func,
            string helpText = "")
            where TMode : AppMode
        {
            return new CommandExecutorWithRunModeGeneric<TMode>(name, helpText, func);
        }

        #endregion

        public virtual string Execute(CommandSpec command)
        {
            CommandExecutor executor;
            if (!_commands.TryGetValue(command.Name, out executor))
            {
                return "Command not found";
            }

            return executor.Execute(command);
        }
    }
}