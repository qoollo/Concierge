using System.Collections.Generic;
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
    public class CommandsTests : AppBuilderTestBase
    {
        private TestExecutableBuilder _executorBuilder;

        protected override AppBuilder GetAppBuilder()
        {
            Installer = GetExecutor();
            _executorBuilder = new TestExecutableBuilder(typeof (UserExecutor), new CommandExecutorProxy());
            return new AppBuilder(_executorBuilder);
        }

        [TestMethod]
        public void DebugMode_ConsoleCommandSource_ExitCommand_Works()
        {
            RunAsync(":debug");

            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));
            Thread.Sleep(500);

            ExitReceived.ShouldBeTrue();
        }

        [TestMethod]
        public void DebugMode_ConsoleCommandSource_GetStartupParametersCommand_Works()
        {
            RunAsync(":debug -s -s -s");

            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("GetStartupArgs",
                new Dictionary<string, string>()))
                .ShouldBeEqualTo("-s -s -s ");
            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));

            Thread.Sleep(500);

            ExitReceived.ShouldBeTrue();
        }

        [TestMethod]
        public void DebugMode_ConsoleCommandSource_CustomCommand_ReturnsOKThreeTimes()
        {
            int count = 0;

            AppBuilder.AddCommand("func", () => count++);

            RunAsync(":debug");

            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("func", new Dictionary<string, string>()))
                .ShouldBeEqualTo("OK");
            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("func", new Dictionary<string, string>()))
                .ShouldBeEqualTo("OK");
            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("func", new Dictionary<string, string>()))
                .ShouldBeEqualTo("OK");

            count.ShouldBeEqualTo(3, "Real = " + count);

            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));

            Thread.Sleep(500);

            ExitReceived.ShouldBeTrue();
        }

        [TestMethod]
        public void DebugMode_ConsoleCommandSource_2EqualCommandsFromInitializationHelper_ChangedCommandCall()
        {
            int flag = 0;

            AppBuilder.AddCommand("func", () => flag = 1);
            AppBuilder.AddCommand("func", () => flag = 2);

            RunAsync(":debug");

            Thread.Sleep(500);

            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("func", new Dictionary<string, string>()))
                .ShouldBeEqualTo("OK");

            flag.ShouldBeEqualTo(2);

            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));

            Thread.Sleep(500);

            ExitReceived.ShouldBeTrue();
        }

        [TestMethod]
        public void DebugMode_ConsoleCommandSource_UnknownCommand_ReturnsUnknownCommandMessage_DoesNotFall()
        {
            RunAsync(":debug");

            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("func", new Dictionary<string, string>()))
                .ShouldBeEqualTo("Command not found");

            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));

            Thread.Sleep(500);

            ExitReceived.ShouldBeTrue();
        }

        [TestMethod]
        public void InteractiveMode_ConsoleCommandSource_EntersDebugModeViaCommand()
        {
            RunAsync(":interactive");

            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("debug", new Dictionary<string, string>()));

            Thread.Sleep(100);

            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("Get", new Dictionary<string, string>()))
                .ShouldBeEqualTo("Test string");

            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));

            Thread.Sleep(500);

            ExitReceived.ShouldBeTrue();
        }

        [TestMethod]
        public void InstallerMode_ConsoleCommandSource_InstallerDefaultArgsWithSomeAction_CountParamsCall()
        {
            int counter = 0;
            AppBuilder.AddStartupParameter("-s", () => counter++);
            AppBuilder.DefaultStartupString = "-s -s :interactive";

            RunAsync("-s -s");

            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));
            Thread.Sleep(500);

            ExitReceived.ShouldBeTrue();

            // 2 раза сработает из-за входной строки и 2 раза в строке по умолчанию
            counter.ShouldBeEqualTo(4);
        }

        [TestMethod]
        public void Run_RenameDebugMode()
        {
            int parameterHandledCount = 0;

            AppBuilder.ChangeStartupParameterName(":debug", "-end");

            AppBuilder.AddStartupParameter(":debug", str =>
            {
                parameterHandledCount++;
                str.ShouldBeEqualTo("testValueS");
            });

            RunAsync(":debug testValueS -end");

            _executorBuilder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));
            Thread.Sleep(500);

            parameterHandledCount.ShouldBeEqualTo(1);
        }
    }
}