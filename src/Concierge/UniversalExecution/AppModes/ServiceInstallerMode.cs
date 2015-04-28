using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Qoollo.Concierge.Commands.Executors;
using Qoollo.Concierge.UniversalExecution.CommandLineArguments;
using Qoollo.Concierge.UniversalExecution.Core;
using Qoollo.Concierge.UniversalExecution.ParamsContext;
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
        private CommandExecutorFromMethod<ServiceInstallerCommand> _commandExecutor;
        private int _timeout = -1;

        public ServiceInstallerMode()
            : base(AppModeNames.Service, "Run program in ServiceInstaller mode", setNewCommands: false)
        {
            _commandExecutor = new CommandExecutorFromMethod<ServiceInstallerCommand>("sevice", "",
                (Func<ServiceInstallerCommand, string>) ParseArguments);
        }

        private string ParseArguments(ServiceInstallerCommand obj)
        {
            if (_mode == ServiceRunMode.none && obj.RunMode == null)
                return "Need run mode argument for command";

            if (_mode == ServiceRunMode.none)
            {
                var ret = CheckAndProcessFirstArgument(obj.RunMode);
                if (!ret)
                    return "Wrong argument: " + obj.RunMode;

                List<string> list = _args.ToList();
                list.RemoveAt(0);
                list.RemoveAt(0);
                _args = list.ToArray();
            }

            _timeout = obj.Timeout;

            if (_timeout != -1)
            {
                List<string> list = _args.ToList();
                list.RemoveAt(0);
                list.RemoveAt(0);
                _args = list.ToArray();
            }

            return HttpStatusCode.OK.ToString();
        }

        private bool CheckAndProcessFirstArgument(string argument)
        {
            ServiceRunMode mode;
            var ret = Enum.TryParse(argument, out mode);
            
            if (ret && !Enum.IsDefined(typeof(ServiceRunMode), mode))
                ret = false;

            if (ret && Enum.IsDefined(typeof (ServiceRunMode), mode))
                _mode = mode;


            return ret;
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

            _mode = ServiceRunMode.none;

            _args = args;

            if (CheckAndProcessFirstArgument(_args[0]))
            {
                var temp = _args.ToList();
                temp.RemoveAt(0);
                _args = temp.ToArray();
            }            

            var spec = StartupParametersManager.Split(_args.Aggregate("service", (current, s) => current + (" " + s)));
            result = _commandExecutor.Execute(spec);
            
            return result == HttpStatusCode.OK.ToString();
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