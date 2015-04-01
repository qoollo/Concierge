using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qoollo.Concierge.Logger
{
    /// <summary>
    /// Custom user logger
    /// </summary>
    public interface IConciergeLogger:IDisposable
    {
        void Log(string message);

        void Log(string message, params object[] args);
    }
}
