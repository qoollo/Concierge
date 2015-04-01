using System;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.UniversalExecution.Core;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge.UniversalExecution.AppModes
{
    internal class DebugMode : AppMode
    {
        public DebugMode(string[] args) : base("debug", "Run program in Debug mode", args)
        {
        }

        public DebugMode()
            : base(AppModeNames.Debug, "Run program in Debug mode")
        {
        }

        protected override void Build(string[] args, ExecutableBuilder executableBuilder)
        {
            if (!WinServiceHelpers.IsServiceInstalled(executableBuilder.WindowsServiceConfig.InstallName))
            {
                RegistrateExecutable(executableBuilder.Build(CommandExecutorProxy, RunMode.Debug,
                    executableBuilder.WindowsServiceConfig.Async, args));

                Console.WriteLine(
                    WinServiceMessages.ServiceStartedMessage(executableBuilder.WindowsServiceConfig.DisplayName));

                RegistrateCommand(CommandExecutorProxy.Build("exit", () => Executable.Stop(), "Stop program"));
            }
        }
    }
}