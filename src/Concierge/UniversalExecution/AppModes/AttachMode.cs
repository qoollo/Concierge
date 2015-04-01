using System;
using System.Linq;
using System.Net;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.Commands.Sources;
using Qoollo.Concierge.UniversalExecution.Core;
using Qoollo.Concierge.UniversalExecution.Decorators;
using Qoollo.Concierge.UniversalExecution.Network;
using Qoollo.Concierge.UniversalExecution.Network.Client;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge.UniversalExecution.AppModes
{
    internal class AttachMode : AppMode
    {
        private readonly IWindowsServiceConfig _winServiceConfig;
        private StableConcurrentConnection<INetCommunication> _connection;

        public AttachMode(IWindowsServiceConfig winServiceConfig)
            : base(AppModeNames.Attach, "Run program in Attach mode")
        {
            if (winServiceConfig == null) throw new ArgumentNullException("winServiceConfig");
            _winServiceConfig = winServiceConfig;
        }

        public override bool Prepare(string[] args, out string result)
        {
            _connection = NetConnector.CreateConnection<INetCommunication>(args, ParamContainer.ServiceHostParameters);

            bool ret = true;
            result = HttpStatusCode.OK.ToString();

            if (!_connection.CanBeUsedForCommunication)
            {
                ret = false;
                result = string.Format("service {0} unavailable", _winServiceConfig.InstallName);
            }

            return ret;
        }

        protected override void Build(string[] args, ExecutableBuilder executableBuilder)
        {
            if (args == null || args.Length == 0)
                FullAttach(executableBuilder);
            else
                AttachForOneCommand(args, executableBuilder);
        }

        private void FullAttach(ExecutableBuilder executableBuilder)
        {
            RegistrateExecutable(executableBuilder.BuildAttachToService(_connection));

            RegistrateCommand(CommandExecutorProxy.Build("exit", () => Executable.Stop(), "Stop program"));
            RegistrateCommand(CommandExecutorProxy.Build("detach", () =>
            {
                SetNextMode(new InteractiveMode());
                Executable.Stop();
            }, "Detach from service"));
        }

        private void AttachForOneCommand(string[] args, ExecutableBuilder executableBuilder)
        {
            IExecutable executable = executableBuilder.BuildAttachToService(_connection, true);
            executable.Start();

            var commandSourceDecorator = executable as CommandSourceDecorator;
            if (commandSourceDecorator != null)
            {
                CommandSource source = commandSourceDecorator.GetSource();
                string command = args.Aggregate("", (current, t) => current + " " + t);
                source.SendCommand(command);
            }
        }
    }
}