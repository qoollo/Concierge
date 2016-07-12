using System;
using System.ServiceProcess;

namespace Qoollo.Concierge
{
    /// <summary>
    /// Default run modes for concierge
    /// </summary>
    public enum DefaultStatupArguments
    {
        /// <summary>
        /// Enable commands, executor is not created. Allow switch to other modes
        /// </summary>
        Interactive,
        /// <summary>
        /// Create executor. Start in console mode.
        /// </summary>
        Debug,
        /// <summary>
        /// Attach to service via wcf. Allow type commands for service executor
        /// </summary>
        Attach,
        /// <summary>
        /// Install executor as service
        /// </summary>
        ServiceInstall,
        /// <summary>
        /// Uninstall executor service
        /// </summary>
        ServiceUnstall,
        /// <summary>
        /// Show all startup arguments. Default mode
        /// </summary>
        Help
    }

    public enum WinServiceAccount
    {
        LocalSystem,
        LocalService,
        NetworkService,
        User
    }

    public static class WinServiceAccountExctension
    {
        public static ServiceAccount ServiceAccount(this WinServiceAccount value)
        {
            return ConvertAccount(value);
        }

        private static ServiceAccount ConvertAccount(WinServiceAccount account)
        {
            return (ServiceAccount)Enum.Parse(typeof(ServiceAccount), account.ToString());
        }
    }
}
