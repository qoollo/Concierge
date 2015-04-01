using System;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.Commands.Routers;
using Qoollo.Concierge.Commands.Sources;
using Qoollo.Concierge.UniversalExecution.Core;

namespace Qoollo.Concierge.FunctionalTests.TestClasses
{
    internal class TestExecutableBuilder : ExecutableBuilder
    {
        public TestExecutableBuilder(CommandExecutorProxy commands) : base(commands)
        {
        }

        public TestExecutableBuilder(Type type, CommandExecutorProxy executorProxy) : base(type, executorProxy)
        {
        }

        public TestExecutableBuilder(IUserExecutable executable, CommandExecutorProxy executorProxy)
            :base(executable, executorProxy)
        {
        }

        public TestCommandSource ConsoleSource { get; private set; }

        protected override CommandSource DebugSource(CommandExecutorProxy commands, string prefix = "")
        {
            ConsoleSource = new TestCommandSource(new LocalRouter(commands));
            return ConsoleSource;
        }

        protected override CommandSource AttachToService(RemoteRouter router, string prefix = "")
        {
            ConsoleSource = new TestCommandSource(router);
            return ConsoleSource;
        }
    }
}