using Qoollo.Concierge.UniversalExecution;

namespace Qoollo.Concierge
{
    public interface IUserExecutable:IExecutable
    {
        IWindowsServiceConfig Configuration { get; }
    }
}
