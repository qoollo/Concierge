using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Qoollo.Concierge.UniversalExecution.CommandLineArguments;
using Qoollo.Concierge.UniversalExecution.Core;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge.UniversalExecution.AppModes
{
    internal enum ServiceRunMode
    {
        install,
        uninstall,
        stop,
        start,
        restart,
        none
    }

    internal class ServiceInstallerMode : AppMode
    {
        private string[] _args = null;
        private ServiceRunMode _mode = ServiceRunMode.none;

        public ServiceInstallerMode()
            : base(AppModeNames.Service, "Run program in ServiceInstaller mode", setNewCommands: false)
        {
        }

        public override bool Prepare(string args, out string result)
        {
            result = HttpStatusCode.OK.ToString();

            bool ret = Enum.TryParse(args, out _mode);
            if (ret && !Enum.IsDefined(typeof (ServiceRunMode), _mode))
                ret = false;

            if (!ret)
                result = "Wrong argument: " + args;

            return ret;
        }

        public override bool Prepare(string[] args, out string result)
        {
            if (args == null || args.Length == 0)
            {
                result = "Need argument for command";
                return false;
            }

            bool ret = Prepare(args[0], out result);

            if (ret)
            {
                List<string> list = args.ToList();
                list.RemoveAt(0);
                _args = list.ToArray();
            }

            return ret;
        }

        protected override void Build(string[] args, ExecutableBuilder executableBuilder)
        {
            _args = _args ?? args;

            IWindowsServiceConfig winServiceConfig = executableBuilder.WindowsServiceConfig;
            var executor = new InstallerBase();

            switch (_mode)
            {
                case ServiceRunMode.install:
                    Install(executor, _args, winServiceConfig);
                    break;
                case ServiceRunMode.uninstall:
                    Uninstall(executor);
                    break;
                case ServiceRunMode.start:
                    WinServiceHelpers.StartService(winServiceConfig.InstallName);
                    break;
                case ServiceRunMode.stop:
                    WinServiceHelpers.StopService(winServiceConfig.InstallName);
                    break;
                case ServiceRunMode.restart:
                    WinServiceHelpers.RestartService(winServiceConfig.InstallName);                    
                    break;
            }
            Console.WriteLine(WinServiceStatus.Get(winServiceConfig.InstallName).ToString());
            _mode = ServiceRunMode.none;
        }

        private void Uninstall(InstallerBase installer)
        {
            installer.UninstallFromWindowsServices();
        }

        private void Install(InstallerBase installer, string[] args, IWindowsServiceConfig winServiceConfig)
        {
            string install = StartupParametersManager.ConcatArgs(args);

            installer.InstallAsWindowsService(install, ParamContainer.ServiceHostParameters.Parameters);
            ParamContainer.ServiceHostParameters.Clear();

            if (winServiceConfig.RestartOnRecover)
                installer.SetRecoveryRestartForWindowsService();

            if (winServiceConfig.StartAfterInstall)
                installer.StartInstalledWindowsService();
        }
    }
}