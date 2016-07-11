using System;
using System.ServiceModel;
using Qoollo.Concierge.Commands.Routers;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.Commands.Sources
{
    internal class WcfCommandSource : CommandSource
    {
        private readonly Uri[] _baseAddress;
        private readonly bool _ignoreRemoteConnectionFail;
        private readonly CommandRouter _commandRouter;
        private ReceiverHost _host;

        public WcfCommandSource(CommandRouter commandRouter, Uri[] baseAddress, bool ignoreRemoteConnectionFail)
            : base(commandRouter)
        {
            _commandRouter = commandRouter;
            _baseAddress = baseAddress;
            _ignoreRemoteConnectionFail = ignoreRemoteConnectionFail;
        }

        public override void StartListening()
        {            
            try
            {
                _host = NetConnector.CreateService(_commandRouter, _baseAddress, Logger);
                _host.Open();
            }
            catch (Exception e)
            {
                Logger.Log(string.Format("Open host. Message = {0}", e.Message));
                if (!_ignoreRemoteConnectionFail)
                    throw;
            }
        }

        public override void StopListening()
        {
            try
            {
                if (_host.State != CommunicationState.Closed)
                    _host.Close();
            }
            catch (Exception)
            {
                if (!_ignoreRemoteConnectionFail)
                    throw;
            }
        }
    }
}