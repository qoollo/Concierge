using System.ServiceProcess;

namespace Qoollo.Concierge
{
    public interface IWindowsServiceConfig
    {
        bool Async { get; set; }

        string DisplayName { set; get; }

        string InstallName { set; get; }

        string Description { set; get; }

        bool RestartOnRecover { set; get; }

        bool StartAfterInstall { set; get; }

        WinServiceAccount Account { set; get; }

        string Password { set; get; }

        string Username { set; get; }
    }
}