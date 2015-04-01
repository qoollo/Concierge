using System;
using System.IO;
using System.Text;
using System.Threading;
using Qoollo.Concierge.Commands.Routers;
using Qoollo.Concierge.UniversalExecution.CommandLineArguments;

namespace Qoollo.Concierge.Commands.Sources
{
    internal class ConsoleCommandSource : CommandSource
    {
        private readonly byte[] _buffer = new byte[10000];
        private readonly AutoResetEvent _event = new AutoResetEvent(false);
        private readonly string _prefix;
        private Stream _console;

        public ConsoleCommandSource(CommandRouter router, string prefix = "")
            : base(router)
        {
            _prefix = prefix;
        }

        public override void StartListening()
        {
            _console = Console.OpenStandardInput();
            Console.Write(_prefix);
            _console.BeginRead(_buffer, 0, _buffer.Length, Callback, _console);
            _event.WaitOne();
        }

        private void Callback(IAsyncResult ar)
        {
            var console = (Stream) ar.AsyncState;
            int count = console.EndRead(ar);

            string str = Encoding.UTF8.GetString(_buffer, 0, count);
            str = str.Remove(str.Length - 2);

            if (!string.IsNullOrWhiteSpace(str))
            {
                Console.WriteLine(RouteCommand(StartupParametersManager.Split(str)));
            }
            if (console.CanRead)
            {
                Console.Write(_prefix);
                console.BeginRead(_buffer, 0, _buffer.Length, Callback, console);
            }
        }

        public override void StopListening()
        {
            _console.Close();
            _event.Set();
        }
    }
}