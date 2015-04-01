using System;
using System.ServiceModel;
using Qoollo.Concierge.Commands.Routers;
using Qoollo.Concierge.Logger;

namespace Qoollo.Concierge.UniversalExecution.Network
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single)]
    internal class NetCommandReceiver : INetCommunication
    {
        private readonly CommandRouter _commandRouter;
        private readonly IConciergeLogger _logger;

        public NetCommandReceiver(CommandRouter commandRouter, IConciergeLogger logger)
        {
            _commandRouter = commandRouter;
            _logger = logger;
        }

        public string SendCommand(CommandSpec command)
        {
            try
            {
                return _commandRouter.Send(command);
            }
            catch (Exception e)
            {
                _logger.Log(e.ToString());
                return e.Message;
            }
        }
    }
}