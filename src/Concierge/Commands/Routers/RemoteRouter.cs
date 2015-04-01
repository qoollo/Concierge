using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Qoollo.Concierge.UniversalExecution.Network;
using Qoollo.Concierge.UniversalExecution.Network.Client;

namespace Qoollo.Concierge.Commands.Routers
{
    internal delegate bool RouteCommands(CommandSpec command, out string result);

    internal class RemoteRouter : CommandRouter
    {
        private readonly CommandExecutorProxy _commandManager;

        private readonly List<RouteCommands> _commandses;
        private readonly StableConcurrentConnection<INetCommunication> _connection;

        public RemoteRouter(CommandExecutorProxy commandManager,
            StableConcurrentConnection<INetCommunication> connection)
        {
            Contract.Requires(connection != null && connection.State == StableConnectionState.Opened);
            Contract.Requires(commandManager != null);
            _commandManager = commandManager;
            _connection = connection;

            _commandses = new List<RouteCommands> {IsLocal, IsHelp, IsExit_or_Detach};
        }

        public override string Send(CommandSpec command)
        {
            foreach (RouteCommands routeCommandse in _commandses)
            {
                string result;
                if (routeCommandse(command, out result))
                    return result;
            }

            return SendToService(command);
        }

        private string SendToService(CommandSpec command)
        {
            if (!_connection.CanBeUsedForCommunication)
                return "Connection can't be used for communications";

            string ret;

            using (ConcurrentRequestTracker<INetCommunication> request = _connection.RunRequest(0))
            {
                ret = !request.CanBeUsedForCommunication
                    ? "Connection can't be used for communications"
                    : request.API.SendCommand(command);
            }
            return ret;
        }

        private bool IsHelp(CommandSpec command, out string result)
        {
            result = string.Empty;

            if (command.Name.Equals("help"))
            {
                result = string.Format("\nLocal:\n{0}\nRemote:\n{1}", _commandManager.Execute(command),
                    SendToService(command));
                return true;
            }

            return false;
        }

        private bool IsExit_or_Detach(CommandSpec command, out string result)
        {
            result = string.Empty;

            if (command.Name.Equals("exit") || command.Name.Equals("detach"))
            {
                result = _commandManager.Execute(command);
                return true;
            }

            return false;
        }

        private bool IsLocal(CommandSpec command, out string result)
        {
            result = string.Empty;

            if (command.Name.Equals("local") && command.Arguments.Count > 0)
            {
                string key = command.Arguments.Keys.ToList().First();
                command.Arguments.Remove(key);

                var comm = new CommandSpec(key, command.Arguments);
                result = _commandManager.Execute(comm);

                return true;
            }

            return false;
        }
    }
}