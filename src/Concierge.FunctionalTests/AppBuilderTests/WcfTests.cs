using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssert;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.Commands.Routers;
using Qoollo.Concierge.FunctionalTests.TestClasses;
using Qoollo.Concierge.Logger;
using Qoollo.Concierge.UniversalExecution.Core;
using Qoollo.Concierge.UniversalExecution.Network;
using Qoollo.Concierge.UniversalExecution.ParamsContext;

namespace Qoollo.Concierge.FunctionalTests.AppBuilderTests
{
    [TestClass]
    public class WcfTests
    {
        private const string Default = "net.tcp://localhost:8000/Concierge";        
        private const string ServiceUri2 = "net.tcp://localhost:8020/Concierge";


        private TRet GetPrivtaeField<TRet>(object obj) where TRet : class
        {
            var list = obj.GetType().GetFields(BindingFlags.Public |
                                              BindingFlags.NonPublic |
                                              BindingFlags.Instance);

            return list.First(x => x.FieldType.FullName == typeof(TRet).ToString()).GetValue(obj) as TRet;
        }

        [TestMethod]
        public void NetConnector_CreateChannelFromConfig_NoConfigSection_ShoudNotBeNull()
        {
            ConfigHelper.UpdateConfig(true);
            string str = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            Trace.WriteLine(str);
            Trace.WriteLine(File.ReadAllText(str));
            
            var factory = NetConnector.CreateChannelFromConfig<INetCommunication>();
            Trace.WriteLine(factory);

            factory.ShouldBeNull();
        }

        [TestMethod]
        public void NetConnector_CreateChannelFromConfig_ShoudNotBeNull()
        {
            ConfigHelper.UpdateConfig();
            NetConnector.CreateChannelFromConfig<INetCommunication>().ShouldNotBeNull();
        }

        [TestMethod]
        public void NetConnector_CreateChannelFromConfig_InValidEndPointName_ShoudBeNull()
        {
            ConfigHelper.UpdateConfig(endpointName: "It's a fake!");
            NetConnector.CreateChannelFromConfig<INetCommunication>().ShouldBeNull();
        }

        [TestMethod]
        public void NetConnector_CreateConnection_WithoutParamsFromConfig_ChooseConfigShoudNotBeNull()
        {
            ConfigHelper.UpdateConfig();

            var app = new AppBuilder();

            app.WithDefaultStartupString("")
                .Run(new string[] {});


            var modeManger = GetPrivtaeField<ModeManager>(app);
            var paramContainer = GetPrivtaeField<ParamContainer>(modeManger);

            var connection = NetConnector.CreateConnection<INetCommunication>(new string[] {},
                paramContainer.ServiceHostParameters).ShouldNotBeNull();

            connection.EndPoint.Address.Uri.OriginalString.ShouldBeEqualTo(ConfigHelper.ClientUri);
        }

        [TestMethod]
        public void NetConnector_CreateConnection_WithParamsAndWitoutConfig_ChooseParamsShoudNotBeNull()
        {
            const string argHost = "net.tcp://localhost:8200/Concierge";
            ConfigHelper.UpdateConfig(true);

            var app = new AppBuilder(true);

            app.WithDefaultStartupString("")
                .Run(new[] { "-host", argHost });


            var modeManger = GetPrivtaeField<ModeManager>(app);
            var paramContainer = GetPrivtaeField<ParamContainer>(modeManger);

            var connection = NetConnector.CreateConnection<INetCommunication>(new string[] { },
                paramContainer.ServiceHostParameters).ShouldNotBeNull();

            connection.EndPoint.Address.Uri.OriginalString.ShouldBeEqualTo(argHost);
        }

        [TestMethod]
        public void NetConnector_CreateConnection_WithParamsAndConfig_ChooseParamsShoudNotBeNull()
        {
            const string argHost = "net.tcp://localhost:8200/Concierge";
            ConfigHelper.UpdateConfig();

            var app = new AppBuilder(true);

            app.WithDefaultStartupString("")
                .Run(new[] { "-host", argHost });


            var modeManger = GetPrivtaeField<ModeManager>(app);
            var paramContainer = GetPrivtaeField<ParamContainer>(modeManger);

            var connection = NetConnector.CreateConnection<INetCommunication>(new string[] { },
                paramContainer.ServiceHostParameters).ShouldNotBeNull();

            connection.EndPoint.Address.Uri.OriginalString.ShouldBeEqualTo(argHost);
        }

        [TestMethod]
        public void NetConnector_CreateServer_WithoutParamWithConfig_ChooseConfig()
        {
            ConfigHelper.UpdateConfig();

            var host = NetConnector.CreateService(new LocalRouter(new CommandExecutorProxy()), new Uri[0], new ConsoleLogger());

            host.Open();
            try
            {
                host.Description.Endpoints.Count.ShouldBeEqualTo(1);
                host.Description.Endpoints[0].Address.Uri.OriginalString.ShouldBeEqualTo(ConfigHelper.ServiceUri);
            }
            finally
            {
                host.Close();
            }
        }

        [TestMethod]
        public void NetConnector_CreateServer_WithParamWithoutConfig_ChooseParams()
        {
            ConfigHelper.UpdateConfig(true);

            var host = NetConnector.CreateService(new LocalRouter(new CommandExecutorProxy()),
                new[] {new Uri(ServiceUri2)}, new ConsoleLogger());

            host.Open();
            try
            {                
                host.Description.Endpoints.Count.ShouldBeEqualTo(1);
                host.Description.Endpoints[0].Address.Uri.OriginalString.ShouldBeEqualTo(ServiceUri2);    
            }
            finally
            {                
                host.Close();
            }
            
        }

        [TestMethod]
        public void NetConnector_CreateServer_WithParamWithConfig_ChooseParams()
        {
            ConfigHelper.UpdateConfig();

            var host = NetConnector.CreateService(new LocalRouter(new CommandExecutorProxy()),
                new[] { new Uri(ServiceUri2) }, new ConsoleLogger());

            host.Open();
            try
            {
                host.Description.Endpoints.Count.ShouldBeEqualTo(1);
                host.Description.Endpoints[0].Address.Uri.OriginalString.ShouldBeEqualTo(ServiceUri2);
            }
            finally
            {
                host.Close();
            }
        }

        [TestMethod]
        public void NetConnector_CreateServer_WithoutParamWithoutConfig_UseDefault()
        {
            ConfigHelper.UpdateConfig(true);

            NetConnector.DefaultUri = new[] {new Uri(ServiceUri2)};
            var host = NetConnector.CreateService(new LocalRouter(new CommandExecutorProxy()), new Uri[0],
                new ConsoleLogger());

            host.Open();
            try
            {
                host.Description.Endpoints.Count.ShouldBeEqualTo(1);
                host.Description.Endpoints[0].Address.Uri.OriginalString.ShouldBeEqualTo(ServiceUri2);
            }
            finally
            {
                host.Close();
            }
        }
    }
}
