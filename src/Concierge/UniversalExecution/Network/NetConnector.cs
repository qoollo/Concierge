using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using Qoollo.Concierge.Commands.Routers;
using Qoollo.Concierge.Logger;
using Qoollo.Concierge.UniversalExecution.Network.Client;
using Qoollo.Concierge.UniversalExecution.ParamsContext;

namespace Qoollo.Concierge.UniversalExecution.Network
{
    internal static class NetConnector
    {
        public static int MaxAsyncQueryCount = 10;
        public static string ConfigSectionName = "localConnection";

        private static Uri[] _uri = {new Uri("net.tcp://localhost:8000/Concierge")};
        public  static Uri[] DefaultUri
        {
            get { return _uri; }
            set { _uri = value; }
        }

        public static ReceiverHost CreateService(CommandRouter router, Uri[] baseAddress, IConciergeLogger logger)
        {
            ReceiverHost host;
            
            if (baseAddress.Length != 0)
            {                
                host = new ReceiverHost(router, logger, typeof (NetCommandReceiver));
                SetEndpointToService(host, baseAddress);
            }
            else
            {
                host = new ReceiverHost(router, logger, typeof(NetCommandReceiver));
                if (host.Description.Endpoints.Count == 0)
                    SetEndpointToService(host, DefaultUri);
            }

            return host;
        }

        private static void SetEndpointToService(ReceiverHost host, Uri[] uri)
        {
            var behavior =
                host.Description.Behaviors.FirstOrDefault(x => x.GetType() == typeof (ServiceMetadataBehavior))
                as ServiceMetadataBehavior ;
            if (behavior != null)
            {
                behavior.HttpGetEnabled = false;
            }

            while (host.Description.Endpoints.Count != 0)
                host.Description.Endpoints.RemoveAt(0);

            host.AddServiceEndpoint(typeof(INetCommunication), new NetTcpBinding(), uri[0]);            
        }

        public static StableConcurrentConnection<TNetContract> CreateConnection<TNetContract>(string[] args,
            ServiceHostParameters serviceHostParameters)
        {
            var channel =
                CreateChannelFromParams<TNetContract>(args, serviceHostParameters) ??
                CreateChannelFromConfig<TNetContract>() ??
                CreateChannelWithDefaultParams<TNetContract>();

            serviceHostParameters.Clear();

            return new StableConcurrentConnection<TNetContract>(channel, MaxAsyncQueryCount, true);
        }

        internal static ChannelFactory<TNetContract> CreateChannelFromParams<TNetContract>(string[] args,
            ServiceHostParameters serviceHostParameters)
        {
            // get uri from startup argument
            Uri uri = serviceHostParameters.Count == 0
                ? null
                : serviceHostParameters.Uri.First();
            // get uri from command argument
            if (uri == null && args != null && args.Length == 1)
                uri = new Uri(args[0]);

            if (uri == null)
                return null;

            return new ChannelFactory<TNetContract>(new NetTcpBinding(), uri.ToString());
        }

        internal static ChannelFactory<TNetContract> CreateChannelWithDefaultParams<TNetContract>()
        {
            var uri = DefaultUri.First();            
            return new ChannelFactory<TNetContract>(new NetTcpBinding(), uri.ToString());
        }

        internal static ChannelFactory<TNetContract> CreateChannelFromConfig<TNetContract>()
        {
            ChannelFactory<TNetContract> channel = null;
            try
            {
                if (!File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile))
                    return null;

                channel = new ChannelFactory<TNetContract>(ConfigSectionName);
            }
            catch (InvalidOperationException e)
            {
            }

            return channel;
        }
    }
}