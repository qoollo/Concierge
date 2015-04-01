using System;
using System.Collections.Generic;

namespace Qoollo.Concierge.Logger
{
    class AggregateLogger:IConciergeLogger
    {
        private readonly List<IConciergeLogger>  _loggers = new List<IConciergeLogger>();

        public void Dispose()
        {
            foreach (var conciergeLogger in _loggers)
            {
                conciergeLogger.Dispose();
            }
        }

        public void Log(string message)
        {
            foreach (var conciergeLogger in _loggers)
            {
                conciergeLogger.Log(message);
            }
        }

        public void Log(string message, params object[] args)
        {
            Log(string.Format(message, args));
        }

        public void AddLogger(IConciergeLogger logger)
        {
            _loggers.Add(logger);
        }
    }
}
