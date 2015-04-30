using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace Qoollo.Concierge.FunctionalTests.TestClasses
{
    static class ConfigHelper
    {        
        public const string ClientUri = "net.tcp://localhost:8010/Concierge";
        public const string ServiceUri = "net.tcp://localhost:8010/Concierge";

        public static void UpdateConfig(bool empty = false, string endpointName = "localConnection", bool addHttpGetEnabled = false)
        {

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var service = config.SectionGroups["system.serviceModel"] as ServiceModelSectionGroup;

            if (service == null)
                return;

            service.Client.Endpoints.Clear();
            service.Services.Services.Clear();
            service.Behaviors.ServiceBehaviors.Clear();

            if (addHttpGetEnabled)
            {
                service.Behaviors.ServiceBehaviors.Add(new ServiceBehaviorElement());
                var behavior =  service.Behaviors.ServiceBehaviors[0];
                behavior.Add(new ServiceMetadataPublishingElement
                {
                    HttpGetEnabled = true
                });
            }

            if (!empty)
            {
                service.Behaviors.ServiceBehaviors.Add(new ServiceBehaviorElement("ConciergeServiceBehavior"));

                service.Services.Services.Add(
                    new ServiceElement("Qoollo.Concierge.UniversalExecution.Network.NetCommandReceiver"));
                service.Services.Services["Qoollo.Concierge.UniversalExecution.Network.NetCommandReceiver"]
                    .BehaviorConfiguration = "ConciergeServiceBehavior";
                service.Services.Services["Qoollo.Concierge.UniversalExecution.Network.NetCommandReceiver"].Endpoints
                    .Add(
                        new ServiceEndpointElement(new Uri(ServiceUri),
                            "Qoollo.Concierge.UniversalExecution.Network.INetCommunication")
                        {
                            Binding = "netTcpBinding"
                        });

                service.Client.Endpoints.Add(new ChannelEndpointElement(
                    new EndpointAddress(ClientUri),
                    "Qoollo.Concierge.UniversalExecution.Network.INetCommunication")
                {
                    Binding = "netTcpBinding",
                    Name = endpointName
                });
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("system.serviceModel");

            ResetConfigMechanism();
        }

        private static void ResetConfigMechanism()
        {
            BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Static;
            typeof(ConfigurationManager)
                .GetField("s_initState", Flags)
                .SetValue(null, 0);

            typeof(ConfigurationManager)
                .GetField("s_configSystem", Flags)
                .SetValue(null, null);

            typeof(ConfigurationManager)
                .Assembly.GetTypes().First(x => x.FullName == "System.Configuration.ClientConfigPaths")
                .GetField("s_current", Flags)
                .SetValue(null, null);
            return;
        }
    }
}
