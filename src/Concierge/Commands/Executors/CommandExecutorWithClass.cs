using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Qoollo.Concierge.UniversalExecution.Network;
using Qoollo.Concierge.Whale;

namespace Qoollo.Concierge.Commands.Executors
{
    internal abstract class CommandExecutorWithClass<TCommand> : CommandExecutor
        where TCommand : UserCommand
    {
        private readonly List<ParameterProperty> _parameters = new List<ParameterProperty>();

        protected CommandExecutorWithClass(string commandName, string description)
            : base(commandName, description)
        {
            _parameters = CommandReflector.GetParameterList<TCommand>();
        }

        public override string Execute(CommandSpec command)
        {
            //Create command
            var tCommand = CommandReflector.CreateInstance<TCommand>();

            tCommand.Arguments = new Dictionary<string, string>(command.Arguments);
            tCommand.CommandName = CommandName;

            // If command is base type
            if (tCommand.GetType() == typeof (UserCommand))
                return ExecuteGeneric(tCommand);

            string result = string.Empty;

            try
            {
                // Set command parameters
                CommandReflector.PrepareInstance(tCommand, _parameters, command);
            }
                // Catch all execetions if command is invalid. (We dont want to crash)
            catch (TargetException ex)
            {
                result = ex.Message;
            }
            catch (TargetParameterCountException ex)
            {
                result = ex.Message;
            }
            catch (TargetInvocationException ex)
            {
                result = ex.Message;
            }
            catch (ArgumentException ex)
            {
                result = ex.Message;
            }
            catch (MethodAccessException ex)
            {
                result = ex.Message;
            }
            catch (InvalidCastException ex)
            {
                result = ex.Message;
            }
            catch (CmdValueForArgumentNotFoundException ex)
            {
                result = ex.Message;
            }
            catch (FormatException ex)
            {
                result = "Invalid default value";
            }


            return !string.IsNullOrEmpty(result) ? result : ExecuteGeneric(tCommand);
        }

        protected abstract string ExecuteGeneric(TCommand command);

        public override string GetHelp()
        {
            var list = new List<string>();
            if (_parameters.Count == 0)
                list.Add(Description);
            else
            {
                list.AddRange(
                    _parameters.Select(parameter => parameter.Attribute.ShortKey + " " + parameter.Attribute.Description));
            }

            return CustomConsoleHelpers.FormatHelp(list);
        }
    }
}