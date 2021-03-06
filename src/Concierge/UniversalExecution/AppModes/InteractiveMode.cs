﻿using Qoollo.Concierge.Commands;
using Qoollo.Concierge.UniversalExecution.Core;

namespace Qoollo.Concierge.UniversalExecution.AppModes
{
    internal class InteractiveMode : AppMode
    {
        public InteractiveMode()
            : base(AppModeNames.Interactive, "Run program in Interactive mode")
        {
        }

        protected override void Build(string[] args, ExecutableBuilder executableBuilder)
        {
            RegistrateExecutable(executableBuilder.BuildInteractiveMode());

            RegistrateCommand(CommandExecutorProxy.Build("exit", () => Executable.Stop(), "Stop program"));
            RegistrateCommand(CommandExecutorProxy.Build("debug", arguments =>
            {
                SetNextMode(new DebugMode(arguments));
                Executable.Stop();
            }, "Run program in Debug mode", DebugMode.GetHelp()));

            RegistrateCommand(CommandExecutorProxy.Build("attach", arguments =>
            {
                var app = new AttachMode(executableBuilder.WindowsServiceConfig);
                string result;
                app.SetParamContainer(ParamContainer);
                if (app.Prepare(arguments, out result))
                {
                    SetNextMode(app);
                    Executable.Stop();
                }
                return result;
            }, "Attach to service"));

            RegistrateCommand(CommandExecutorProxy.Build("service",
                arguments =>
                {
                    var app = new ServiceInstallerMode();
                    app.SetParamContainer(ParamContainer);
                    string result;
                    if (app.Prepare(arguments, out result))
                    {
                        app.Start(arguments, executableBuilder);
                    }
                    return result;
                }, "install/uninstall/restart program as service", ServiceInstallerMode.GetHelp()));
        }

        public override string StartInfo
        {
            get
            {
                return string.Format(
                        "Start {0} mode. Type help as command or :help as startup argumnet for addition info", Name);
            }
        }
    }
}