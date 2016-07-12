using System;
using System.Collections.Generic;
using System.Linq;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.UniversalExecution.Core;
using Qoollo.Concierge.Whale;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge.UniversalExecution.AppModes
{
    internal class DebugMode : AppMode
    {
        /// <summary>
        ///If this argument is provided, program will exit after complete
        /// </summary>
        public const string DefaultExitOnCompleteArg = "-noi";

        public static string ModeInfo = "Run program in Debug mode (Add -noi arg for exit after complete)";

        public DebugMode(string[] args) : base("debug", ModeInfo, args)
        {
        }

        public DebugMode() : base(AppModeNames.Debug, ModeInfo)
        {
        }

        protected override void Build(string[] args, ExecutableBuilder executableBuilder)
        {
            if (!WinServiceHelpers.IsServiceInstalled(executableBuilder.WindowsServiceConfig.InstallName))
                StartAsDebug(args, executableBuilder);
            else if (WinServiceHelpers.IsServiceStopped(executableBuilder.WindowsServiceConfig.InstallName))
            {
                Console.WriteLine(WinServiceStatus.Get(executableBuilder.WindowsServiceConfig.InstallName).ToString());
                StartAsDebug(args, executableBuilder);
            }
            else
                Console.WriteLine(WinServiceStatus.Get(executableBuilder.WindowsServiceConfig.InstallName).ToString());
        }

        private void StartAsDebug(string[] args, ExecutableBuilder executableBuilder)
        {
            var interactive = args == null || !args.Contains(DefaultExitOnCompleteArg);

            var executable = executableBuilder.Build(CommandExecutorProxy, RunMode.Debug,
                executableBuilder.WindowsServiceConfig.Async, args, interactive: interactive);

            if (!interactive)
            {
                executable.Start();
                return;
            }

            RegistrateExecutable(executable);

            Console.WriteLine(
                WinServiceMessages.ServiceStartedMessage(executableBuilder.WindowsServiceConfig.DisplayName));

            RegistrateCommand(CommandExecutorProxy.Build("exit", () => Executable.Stop(), "Stop program"));
        }

        public static string GetHelp()
        {
            var list = new List<string>
            {
                "noi program will exit after complete"
            };

            return CustomConsoleHelpers.FormatHelp(list);
        }
    }
}