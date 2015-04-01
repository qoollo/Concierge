using System.Net;
using Qoollo.Concierge.UniversalExecution.CommandLineArguments;
using Qoollo.Concierge.UniversalExecution.Core;

namespace Qoollo.Concierge.UniversalExecution.AppModes
{
    /// <summary>
    ///     Windows Service mode for Installed service only.
    /// </summary>
    internal class WinServiceMode : AppMode
    {
        private string _install;

        public WinServiceMode()
            : base(AppModeNames.Win, "Run program in WinService mode")
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
            string[] arguments = StartupParametersManager.TransformArgs(_install);
            RegistrateExecutable(executableBuilder.Build(CommandExecutorProxy, RunMode.Service,
                executableBuilder.WindowsServiceConfig.Async,
                arguments, ParamContainer.ServiceHostParameters.Uri));
        }
    }
}