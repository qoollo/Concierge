using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssert;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qoollo.Concierge.Attributes;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.Commands.Executors;
using Qoollo.Concierge.FunctionalTests.TestClasses;
using Qoollo.Concierge.UniversalExecution.Network;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge.FunctionalTests.AppBuilderTests
{
    [TestClass]
    public class CommandExecutorTests
    {
        [TestMethod]
        public void Execute_CommandSpecWithClass_OneShortParam_Test()
        {
            bool flag = false;

            var command = new CommandExecutorFromMethod<StringCommand>("name", "",
                testCommand =>
                {
                    flag = true;
                    testCommand.AField.ShouldBeEqualTo("value");
                });

            var send = new CommandSpec("name", new Dictionary<string, string>
            {
                {"-a", "value"},
            });

            command.Execute(send).ShouldBeEqualTo(HttpStatusCode.OK.ToString());

            flag.ShouldBeTrue();
        }

        [TestMethod]
        public void Execute_CommandSpecWithClass_OneLongParam_Test()
        {
            bool flag = false;

            var command = new CommandExecutorFromMethod<StringCommand>("name", "",
                testCommand =>
                {
                    flag = true;
                    testCommand.AField.ShouldBeEqualTo("value");
                });

            var send = new CommandSpec("name", new Dictionary<string, string>
            {
                {"--aaaaaa", "value"}
            });

            command.Execute(send).ShouldBeEqualTo(HttpStatusCode.OK.ToString());

            flag.ShouldBeTrue();
        }

        [TestMethod]
        public void Execute_CommandSpecWithClass_DefaultParam_Test()
        {
            bool flag = false;

            var command = new CommandExecutorFromMethod<StringCommand>("name", "",
                testCommand =>
                {
                    flag = true;
                    testCommand.AField.ShouldBeEqualTo("test");
                });

            var send = new CommandSpec("name", new Dictionary<string, string>());

            command.Execute(send).ShouldBeEqualTo(HttpStatusCode.OK.ToString());

            flag.ShouldBeTrue();
        }

        [TestMethod]
        public void Execute_CommandSpecWithClass_IsRequerd_ParameterRequredMessage()
        {
            bool flag = false;

            var command = new CommandExecutorFromMethod<IntCommand>("name", "",
                testCommand =>
                {
                    flag = true;
                    testCommand.AField.ShouldBeEqualTo(4);
                });

            var send = new CommandSpec("name", new Dictionary<string, string>());

            command.Execute(send).ShouldBeEqualTo("Parameter -b is requred");
            flag.ShouldBeFalse();
        }

        [TestMethod]
        public void Execute_CommandSpecWithClass_IntParameter_ParsedIntField()
        {
            bool flag = false;

            var command = new CommandExecutorFromMethod<IntCommand>("name", "",
                testCommand =>
                {
                    flag = true;
                    testCommand.AField.ShouldBeEqualTo(10);
                });

            var send = new CommandSpec("name", new Dictionary<string, string>
            {
                {"-b", "10"}
            });

            command.Execute(send).ShouldBeEqualTo(HttpStatusCode.OK.ToString());

            flag.ShouldBeTrue();
        }

        [TestMethod]
        public void Execute_CommandSpecWithAttribute_ReturnValue_CreatedCommandExecutor()
        {
            var classForCommands = new ClassForCommands();
            MethodInfo info = classForCommands.GetType().GetMethods().First(x => x.Name == "AMethod");

            var command = new CommandExecutorFromAttribute<StringCommand>("aTestCommand", "", classForCommands, info);

            var send = new CommandSpec("name", new Dictionary<string, string>
            {
                {"-a", "value"},
            });

            command.Execute(send).ShouldBeEqualTo(string.Empty);

            classForCommands.Flag.ShouldBeTrue();
        }

        [TestMethod]
        public void Execute_CommandSpecWithAttribute_NoReturnValueAndBaseCommand_CreatedCommandExecutor()
        {
            var classForCommands = new ClassForCommands();
            MethodInfo info = classForCommands.GetType().GetMethods().First(x => x.Name == "BMethod");

            var command = new CommandExecutorFromAttribute<UserCommand>("bTestCommand", "", classForCommands, info);

            var send = new CommandSpec("name", new Dictionary<string, string>
            {
                {"-b", "4"},
            });

            command.Execute(send).ShouldBeEqualTo(HttpStatusCode.OK.ToString());

            classForCommands.Flag.ShouldBeTrue();
        }

        [TestMethod]
        public void Execute_CommandSpecWithAttribute_FindCommand_CommandFindedInInstance()
        {
            var classForCommands = new ClassForCommands();
            var cmdManager = new CommandExecutorProxy();

            cmdManager.AddFromInstance(classForCommands);

            var send = new CommandSpec("aTestCommand2", new Dictionary<string, string>
            {
                {"-b", "4"},
            });

            cmdManager.Execute(send).ShouldBeEqualTo(HttpStatusCode.OK.ToString());
            classForCommands.Flag.ShouldBeTrue();
        }

        [TestMethod]
        public void Execute_CommandSpecWithAttribute_FindCommandWithReturnValue_CalledCommand()
        {
            var classForCommands = new ClassForCommands();
            var cmdManager = new CommandExecutorProxy();

            cmdManager.AddFromInstance(classForCommands);

            var send = new CommandSpec("aTestCommand", new Dictionary<string, string>());

            cmdManager.Execute(send).ShouldBeEqualTo(string.Empty);
            classForCommands.Flag.ShouldBeTrue();
        }

        [TestMethod]
        public void DebugMode_InitializationHelper_FloatAndDoubleCommand_CreatedCommand()
        {
            AssemblyHelper.EntryAssembly = Assembly.GetExecutingAssembly();
            var builder = new TestExecutableBuilder(typeof (UserExecutor), new CommandExecutorProxy());
            var helper = new AppBuilder(builder);

            bool isExit = false;
            Task.Factory.StartNew(() =>
            {
                helper.Run("-debug".Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
                isExit = true;
            });

            Thread.Sleep(500);

            builder.ConsoleSource.SendCommand("FloatAndDouble -a 3 -b 4").ShouldBeEqualTo("3 4");
            builder.ConsoleSource.SendCommand("FloatAndDouble -a 3.4 -b 4.4").ShouldBeEqualTo("3,4 4,4");
            builder.ConsoleSource.SendCommand("FloatAndDouble -a 3").ShouldBeEqualTo("Parameter -b is requred");

            builder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));
            Thread.Sleep(500);

            isExit.ShouldBeTrue();
        }

        [TestMethod]
        public void DebugMode_InitializationHelper_WrongDefaultParam_InvalidDefaultValueMessage()
        {
            AssemblyHelper.EntryAssembly = Assembly.GetExecutingAssembly();

            var builder = new TestExecutableBuilder(typeof (UserExecutor), new CommandExecutorProxy());

            var helper =
                new AppBuilder(builder);

            bool isExit = false;
            Task.Factory.StartNew(() =>
            {
                helper.Run("-debug".Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
                isExit = true;
            });

            Thread.Sleep(500);

            builder.ConsoleSource.SendCommand("WrongDefaultParam").ShouldBeEqualTo("Invalid default value");

            builder.ConsoleSource.SendCommand(new CommandSpec("exit", new Dictionary<string, string>()));
            Thread.Sleep(500);

            isExit.ShouldBeTrue();
        }
    }

    internal class ClassForCommands
    {
        public bool Flag = false;

        [CommandHandler("aTestCommand")]
        public string AMethod(StringCommand command)
        {
            Flag = true;
            return string.Empty;
        }

        [CommandHandler("aTestCommand2")]
        public void BMethod(UserCommand command)
        {
            Flag = true;
        }
    }
}