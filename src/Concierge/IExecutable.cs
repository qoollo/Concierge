using System;

namespace Qoollo.Concierge
{    
    public interface IExecutable : IDisposable
    {
        void Start();

        void Stop();
    }
}