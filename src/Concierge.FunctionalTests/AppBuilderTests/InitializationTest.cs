using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssert;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.FunctionalTests.TestClasses;
using Qoollo.Concierge.UniversalExecution;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.FunctionalTests.AppBuilderTests
{
    [TestClass]
    public class InitializationTest:AppBuilderTestBase
    {        
        [TestMethod]
        public void AppBuilder_InitializeFromType_GetStartupArgs_EqualToRunParams()
        {
            var executorBuilder = new TestExecutableBuilder(typeof (UserExecutor), new CommandExecutorProxy());
            AppBuilder = new AppBuilder(executorBuilder);
            RunAsync(":debug -s -s -s");

            executorBuilder.ConsoleSource.SendCommand(new CommandSpec("GetStartupArgs",
                new Dictionary<string, string>()))
                .ShouldBeEqualTo("-s -s -s ");
            executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));

            Thread.Sleep(500);

            ExitReceived.ShouldBeTrue();
        }

        [TestMethod]
        public void AppBuilder_InitializeFromInstance_GetStartupArgs_EqualToRunParams()
        {
            var executorBuilder = new TestExecutableBuilder(new UserExecutor(), new CommandExecutorProxy());
            AppBuilder = new AppBuilder(executorBuilder);
            RunAsync(":debug -s -s -s");

            executorBuilder.ConsoleSource.SendCommand(new CommandSpec("GetStartupArgs",
                new Dictionary<string, string>()))
                .ShouldBeEqualTo("-s -s -s ");
            executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));

            Thread.Sleep(500);

            ExitReceived.ShouldBeTrue();
        }

        [TestMethod]
        public void AppBuilder_InitializeByAttribute_GetStartupArgs_EqualToRunParams()
        {
            var executorBuilder = new TestExecutableBuilder(new CommandExecutorProxy());
            AppBuilder = new AppBuilder(executorBuilder);
            RunAsync(":debug -s -s -s");

            executorBuilder.ConsoleSource.SendCommand(new CommandSpec("GetStartupArgs",
                new Dictionary<string, string>()))
                .ShouldBeEqualTo("-s -s -s ");
            executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));

            Thread.Sleep(500);

            ExitReceived.ShouldBeTrue();
        }

        [TestMethod]
        public void AppBuilder_InitializeByFluent_GetStartupArgs_EqualToRunParams()
        {
            var initArgs = "";
            var executorBuilder = new TestExecutableBuilder(new CommandExecutorProxy());
            AppBuilder = new AppBuilder(executorBuilder)
                .UseExecutor(t =>
                {
                    t.OnStart(() => { });
                    t.OnInit(args=> initArgs = args.Aggregate("", (current, s) => current + (s + " ")));
                });
            RunAsync(":debug -s -s -s");

            initArgs.ShouldBeEqualTo("-s -s -s ");

           Thread.Sleep(1000);
            executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));

            Thread.Sleep(500);

            ExitReceived.ShouldBeTrue();
        }

        [TestMethod]
        public void AppBuilder_InitializeByTypeWithFluent_GetStartupArgs_EqualToRunParams_UseTypeInit()
        {
            var initArgs = "";
            var executorBuilder = new TestExecutableBuilder(typeof(UserExecutor), new CommandExecutorProxy());
            AppBuilder = new AppBuilder(executorBuilder)
                .UseExecutor(t =>
                {
                    t.OnStart(() => { });
                    t.OnInit(args => initArgs = args.Aggregate("", (current, s) => current + (s + " ")));
                });
            RunAsync(":debug -s -s -s");

            initArgs.ShouldBeEqualTo("");

            executorBuilder.ConsoleSource.SendCommand(new CommandSpec("GetStartupArgs",
                new Dictionary<string, string>()))
                .ShouldBeEqualTo("-s -s -s ");

            executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));

            Thread.Sleep(500);

            ExitReceived.ShouldBeTrue();
        }

        [TestMethod]
        public void AppBuilder_InitializeByInstanceWithFluent_GetStartupArgs_EqualToRunParams_UseInstanceInit()
        {
            var initArgs = "";
            var executorBuilder = new TestExecutableBuilder(new UserExecutor(), new CommandExecutorProxy());
            AppBuilder = new AppBuilder(executorBuilder)
                .UseExecutor(t =>
                {
                    t.OnStart(() => { });
                    t.OnInit(args => initArgs = args.Aggregate("", (current, s) => current + (s + " ")));
                });
            RunAsync(":debug -s -s -s");

            initArgs.ShouldBeEqualTo("");

            executorBuilder.ConsoleSource.SendCommand(new CommandSpec("GetStartupArgs",
                new Dictionary<string, string>()))
                .ShouldBeEqualTo("-s -s -s ");

            executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));

            Thread.Sleep(500);

            ExitReceived.ShouldBeTrue();
        }
    }
}
