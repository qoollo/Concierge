using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Concierge.TestServiceInstaller;
using FluentAssert;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.FunctionalTests.TestClasses;
using Qoollo.Concierge.UniversalExecution.Core;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge.FunctionalTests.AppBuilderTests
{
    /// <summary>
    /// Summary description for ServiceTests
    /// </summary>
    [TestClass]
    public class ServiceTests
    {        
        private const string ServiceName = "QoolloEmptyServiceTest";
        private const string ServiceHost = "net.tcp://localhost:8000/Concierge";        
        private const string ServiceStartup = "-s -s -s ";

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var path = ExecutionHelper.GetExePath();

            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = @":service install " + ServiceStartup
            };
            Process.Start(startInfo);

            Thread.Sleep(2000);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            var path = ExecutionHelper.GetExePath();

            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = @":service uninstall"
            };
            Process.Start(startInfo);
        }

        private TestExecutableBuilder RunAsync(string args, int timeout = 2000)
        {
            var executorBuilder = new TestExecutableBuilder(new CommandExecutorProxy());
            var appBuilder =  new AppBuilder(executorBuilder, true)
                .WithWinServiceProps(t => t.InstallName = ServiceName);            

            Task.Factory.StartNew(() =>
                appBuilder.Run(args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)));

            Thread.Sleep(timeout);

            return executorBuilder;
        }

        private string SendCommand(TestExecutableBuilder executable, string command)
        {
            var str = executable.ConsoleSource.SendCommand(command);
            Thread.Sleep(500);
            return str;
        }

        [TestMethod]
        public void WinServiceHelpers_IsServiceInstalled_ShouldBeTrue()
        {
            WinServiceHelpers.IsServiceInstalled(ServiceName).ShouldBeTrue();
        }
        
        [TestMethod]
        public void AttachToService_WrongHost_ServiceUnavailable()
        {
            ConsoleLogs.UseStream();
            var executor = RunAsync("-host net.tcp://localhost:8010/Concierge :attach", 6000);
            ConsoleLogs.ReadLine().ShouldBeEqualTo(string.Format("service {0} unavailable", ServiceName));
        }

        [TestMethod]
        public void AttachToService_RightHost_SendCommand_GetTestStringFromService()
        {            
            var executor = RunAsync("-host " + ServiceHost + " :attach");
            
            SendCommand(executor, "Get").ShouldBeEqualTo("Test string");
            SendCommand(executor, "exit");
        }

        [TestMethod]
        public void AttachToService_GetStartupArgs()
        {
            var executor = RunAsync("-host " + ServiceHost + " :attach");

            SendCommand(executor, "GetStartupArgs").ShouldBeEqualTo(ServiceStartup);
            SendCommand(executor, "exit");
        }        
    }
}
