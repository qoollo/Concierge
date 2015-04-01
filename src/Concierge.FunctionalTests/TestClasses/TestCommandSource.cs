using System.Threading;
using Qoollo.Concierge.Commands.Routers;
using Qoollo.Concierge.Commands.Sources;
using Qoollo.Concierge.UniversalExecution.CommandLineArguments;
using Qoollo.Concierge.UniversalExecution.Network;

namespace Qoollo.Concierge.FunctionalTests.TestClasses
{
    internal class TestCommandSource : CommandSource
    {
        private readonly AutoResetEvent _event = new AutoResetEvent(false);
        private CommandSpec _command;
        private string _commandResult = string.Empty;
        private bool _stop;

        public TestCommandSource(CommandRouter commandRouter) : base(commandRouter)
        {
        }

        public override void StartListening()
        {
            _stop = false;
            while (!_stop)
            {
                _event.WaitOne();

                _commandResult = RouteCommand(_command);
                _event.Set();
            }
        }

        public string SendCommand(CommandSpec command)
        {
            _command = command;

            _event.Set();
            _event.Reset();

            _event.WaitOne();

            return _commandResult;
        }

        public string SendCommand(string str)
        {
            _command = StartupParametersManager.Split(str);

            _event.Set();            

            _event.WaitOne();

            return _commandResult;
        }

        public override void StopListening()
        {
            _stop = true;
            _event.Set();
        }
    }
}