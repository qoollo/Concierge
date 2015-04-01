using System;
using Qoollo.Concierge.Commands.Routers;
using Qoollo.Concierge.Logger;
using Qoollo.Concierge.UniversalExecution.CommandLineArguments;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.Commands.Sources
{
    /// <summary>
    /// Command source in system. Generate command by itself
    /// </summary>
    internal abstract class CommandSource
    {
        private readonly CommandRouter _commandRouter;
        public IConciergeLogger Logger;

        protected CommandSource(CommandRouter commandRouter)
        {
            _commandRouter = commandRouter;
        }

        public abstract void StartListening();
        public abstract void StopListening();

        public void SendCommand(string command)
        {
            Console.WriteLine(RouteCommand(StartupParametersManager.Split(command)));
        }

        protected string RouteCommand(CommandSpec command)
        {
            return _commandRouter.Send(command);
        }
    }
}