using System;
using System.ServiceProcess;

namespace Qoollo.Concierge
{
    public enum DefaultStatupArguments
    {
        Interactive,
        Debug,
        Attach,
        ServiceInstall,
        ServiceUnstall,
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
