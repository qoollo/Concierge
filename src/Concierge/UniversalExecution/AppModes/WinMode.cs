using System;
using System.IO;
using System.Net;
using ConsoleHelpers.CommandLineArguments;
using ConsoleHelpers.UniversalExecution.Core;
using ConsoleHelpers.UniversalExecution.Decorators;

namespace ConsoleHelpers.UniversalExecution.AppModes
{
    internal class WinMode : AppMode
    {
        private string _install;

        public WinMode()
            : base("win", "Внутренний параметр")
        {
        }

        public override bool Prepare(string args, out string result)
        {
            result = HttpStatusCode.OK.ToString();
            _install = args;
            return true;
        }

        protected override void Build(string[] args, ExecutableBuilder executableBuilder)
        {            
            var arguments = StartupParametersManager.TransformArgs(_install);
            RegistrateExecutable(executableBuilder.Build(CommandExecutorProxy, RunMode.Service,
                executableBuilder.WindowsServiceConfig.Async,
                arguments, ParamContainer.ServiceHostParameters.Uri));           
        }
    }
}
