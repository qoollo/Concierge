using System;
using System.Linq;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.Commands.Sources;
using Qoollo.Concierge.UniversalExecution.Core;
using Qoollo.Concierge.UniversalExecution.Decorators;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge.UniversalExecution.AppModes
{
    internal class DebugMode : AppMode
    {
        public const string DefaultExitOnCompleteArg = "-noi";
        public DebugMode(string[] args) : base("debug", "Run program in Debug mode", args)
        {
            NonInteractive = DefaultExitOnCompleteArg;
        }

        public DebugMode()
            : base(AppModeNames.Debug, "Run program in Debug mode")
        {
            NonInteractive = DefaultExitOnCompleteArg;
        }
      
        /// <summary>
        ///If this argument is provided, program will exit after complete
        /// </summary>
        public string NonInteractive { get; set; }
        
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
            var interactive = args == null || !args.Contains(NonInteractive);

            var executable = executableBuilder.Build(CommandExecutorProxy, RunMode.Debug,
                executableBuilder.WindowsServiceConfig.Async, args,interactive:interactive);

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
    }
}