using System;
using System.ServiceProcess;

namespace Qoollo.Concierge
{
    public class WinServiceConfig : IWindowsServiceConfig
    {
        public const string UserDefault = null;
        public const string PassDefault = null;
        public const bool RecoverDefault = true;
        public const bool StartDefault = true;
        public const bool AsyncStartDefault = true;
        public const int ServiceOperationsTimeoutMlsDefault = 10000;

        private string _username;

        public WinServiceConfig()
        {
            RestartOnRecover = RecoverDefault;
            StartAfterInstall = StartDefault;
            Account = WinServiceAccount.LocalSystem;
            Username = UserDefault;
            Password = PassDefault;
            Async = AsyncStartDefault;
            ServiceOperationsTimeoutMls = ServiceOperationsTimeoutMlsDefault;
        }

        public string DisplayName { get; set; }
        public string InstallName { get; set; }
        public string Description { get; set; }
        public bool RestartOnRecover { get; set; }
        public bool StartAfterInstall { get; set; }
        public bool Async { get; set; }

        public WinServiceAccount Account { get; set; }
        public string Password { get; set; }

        public string Username
        {
            get { return _username; }
            set {
                _username = value;
                if (!string.IsNullOrWhiteSpace(_username) && !value.Contains(@"\"))
                {
                    Account = WinServiceAccount.User;
                    _username = @".\" + _username;
                }
            }
        }

        public int ServiceOperationsTimeoutMls { get; set; }
    }
}