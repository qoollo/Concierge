using System;
using System.ServiceModel;
using Qoollo.Concierge.Commands.Routers;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.Commands.Sources
{
    internal class WcfCommandSource : CommandSource
    {
        private readonly Uri[] _baseAddress;
        private readonly CommandRouter _commandRouter;
        private ReceiverHost _host;

        public WcfCommandSource(CommandRouter commandRouter, Uri[] baseAddress) : base(commandRouter)
        {
            _commandRouter = commandRouter;
            _baseAddress = baseAddress;
        }

        public override void StartListening()
        {
            _host = NetConnector.CreateService(_commandRouter, _baseAddress, Logger);
            _host.Open();
        }

        public override void StopListening()
        {
            if (_host.State != CommunicationState.Closed)
                _host.Close();
        }
    }
}