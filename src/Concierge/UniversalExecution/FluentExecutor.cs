using System;
using System.Data;

namespace Qoollo.Concierge.UniversalExecution
{
    public class FluentExecutor
    {
        internal Action Start { get; private set; }
        internal Action Stop { get; private set; }
        internal Action<string[]> Init { get; private set; }

        private void IsValid()
        {
            if (Stop != null && Start == null)
                throw new InvalidExpressionException("Need start action");

            if (Init != null && Start == null)
                throw new InvalidExpressionException("Need start action");
        }

        internal bool IsInitialized()
        {
            bool ret = Stop != null || Start != null || Init != null;

            if (ret)
                IsValid();

            return ret;
        }

        public void OnStart(Action action)
        {
            Start = action;
        }

        public void OnStop(Action action)
        {
            Stop = action;
        }

        public void OnInit(Action<string[]> action)
        {
            Init = action;
        }
    }
}