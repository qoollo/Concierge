using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using FluentAssert;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.FunctionalTests.TestClasses;
using Qoollo.Concierge.UniversalExecution;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.FunctionalTests.AppBuilderTests
{
    [TestClass]
    public class FluentTests : AppBuilderTestBase
    {
        private TestExecutableBuilder _executorBuilder;

        protected override AppBuilder GetAppBuilder()
        {
            Installer = GetExecutor();
            _executorBuilder = new TestExecutableBuilder(new CommandExecutorProxy());
            return new AppBuilder(_executorBuilder);
        }

        [TestMethod]
        public void AppBuilder_Fluent_SetDefaultStartupString()
        {
            const string defaultString = "123123123123123";

            AppBuilder.WithDefaultStartupString(defaultString)
                .DefaultStartupString.ShouldBeEqualTo(defaultString);
        }

        [TestMethod]
        public void AppBuilder_Fluent_DebugFromUseExecutor_StartStopActions()
        {
            bool isStart = false;
            bool isStop = false;

            Task.Factory.StartNew(() => AppBuilder
                .WithWinServiceProps(t => t.Async = true)
                .UseExecutor(t =>
                {
                    t.OnStart(() => isStart = true);
                    t.OnStop(() => isStop = true);
                }).Run(new[] {":debug"}));
            Thread.Sleep(500);

            isStart.ShouldBeTrue();
            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));
            Thread.Sleep(500);

            isStop.ShouldBeTrue();
        }

        [TestMethod]
        public void AppBuilder_Fluent_DebugFromUseExecutor_StartNoStopActions()
        {
            bool isStart = false;

            Task.Factory.StartNew(() => AppBuilder
                .WithWinServiceProps(t => t.Async = true)
                .UseExecutor(t => t.OnStart(() => isStart = true)).Run(new[] {":debug"}));
            Thread.Sleep(500);

            isStart.ShouldBeTrue();
            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));
            Thread.Sleep(500);
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidExpressionException))]
        public void AppBuilder_Fluent_DebugFromUseExecutor_OnlyStopActions()
        {
            AppBuilder
                .WithWinServiceProps(t => t.Async = true)
                .UseExecutor(t => t.OnStop(() => { })).Run(new[] {":debug"});
        }

        [TestMethod]
        public void AppBuilder_Fluent_DebugFromUseExecutor_StartStopStartupActionsWithArgs()
        {
            bool isStart = false;
            bool isStop = false;
            bool isStartup = false;

            Task.Factory.StartNew(() => AppBuilder
                .WithWinServiceProps(t => t.Async = true)
                .UseExecutor(t =>
                {
                    t.OnStart(() => isStart = true);
                    t.OnInit(args =>
                    {
                        args[0].ShouldBeEqualTo("-s");
                        isStartup = true;
                    });

                    t.OnStop(() => isStop = true);
                }).Run(new[] {":debug", "-s"}));
            Thread.Sleep(500);

            isStartup.ShouldBeTrue();
            isStart.ShouldBeTrue();
            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));
            Thread.Sleep(500);

            isStop.ShouldBeTrue();
        }

        [TestMethod]
        public void AppBuilder_Fluent_DebugFromUseExecutor_StartStopActionsWithArgs()
        {
            bool isStart = false;
            bool isStop = false;

            Task.Factory.StartNew(() => AppBuilder
                .WithWinServiceProps(t => t.Async = true)
                .UseExecutor(t =>
                {
                    t.OnStart(() => isStart = true);
                    t.OnStop(() => isStop = true);
                }).Run(new[] {":debug", "-s"}));
            Thread.Sleep(500);

            isStart.ShouldBeTrue();
            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));
            Thread.Sleep(500);

            isStop.ShouldBeTrue();
        }
    }
}