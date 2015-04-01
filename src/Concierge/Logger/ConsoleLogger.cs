using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qoollo.Concierge.Logger
{
    class ConsoleLogger:IConciergeLogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void Log(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        public void Dispose()
        {
            
        }
    }
}
