using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Qoollo.Concierge.Logger
{
    class FileLogger:IConciergeLogger
    {
        private readonly string _filePath;
        private readonly BlockingCollection<string> _messages = new BlockingCollection<string>();
        private readonly CancellationTokenSource _token;

        public FileLogger(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
                _filePath = filePath;
            else
                _filePath = AppDomain.CurrentDomain.BaseDirectory + "Concierge.log";

            _token = new CancellationTokenSource();
            Task.Factory.StartNew(WriteMethod);
        }

        public void Log(string message)
        {
            _messages.Add(message);
        }

        public void Log(string message, params object[] args)
        {
            Log(string.Format(message, args));
        }

        private void WriteMethod()
        {

            try
            {
                while (!_token.IsCancellationRequested)
                {
                    var message = _messages.Take(_token.Token);

                    using (var writer = new StreamWriter(_filePath, true))
                    {
                        writer.WriteLine(message);

                    }

                }
            }
            catch (OperationCanceledException)
            {
            }

        }

        public void Dispose()
        {
            _token.Cancel();
        }
    }
}
