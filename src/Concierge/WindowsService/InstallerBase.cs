using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using Qoollo.Concierge.Logger;
using Qoollo.Concierge.UniversalExecution.Core;

namespace Qoollo.Concierge.WindowsService
{
    /// <summary>
    /// WinService main work (Install, run ..)
    /// </summary>
    [RunInstaller(true)]
    public class InstallerBase : Installer
    {
        internal static IWindowsServiceConfig WinServiceConfig;
        internal static IConciergeLogger Logger;
        private readonly InstallersManager _installersManager;

        public InstallerBase()
        {
            _installersManager = new InstallersManager(this, AssemblyHelper.EntryAssembly);
        }

        #region Service Before Events

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            string install = null;
            string startupParameter = null;
            if (Context != null)
                ParseContext(WinServiceConfig, Context.Parameters, out install, out startupParameter);

            _installersManager.UpdateInstallersFromConfig(WinServiceConfig);        
            _installersManager.SetServiceStartParameters(install, startupParameter);
            base.OnBeforeInstall(savedState);
        }

        /// <summary>
        ///     Raises the <see cref="E:System.Configuration.Install.Installer.BeforeUninstall" /> event.
        /// </summary>
        /// <param name="savedState">
        ///     An <see cref="T:System.Collections.IDictionary" /> that contains the state of the computer
        ///     before the installers in the <see cref="P:System.Configuration.Install.Installer.Installers" /> property uninstall
        ///     their installations.
        /// </param>
        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            _installersManager.UpdateInstallersFromConfig(WinServiceConfig);
            base.OnBeforeUninstall(savedState);
        }

        #endregion

        #region Api

        public void RunAsWinService(IExecutable executable)
        {
            ServiceBase.Run(new ServiceBase[]
            {
                new WinServiceBase(executable, WinServiceConfig.InstallName){Logger = Logger}
            });
        }

        public void InstallAsWindowsService(string install, string serviceStartup)
        {
            WinServiceHelpers.InstallWindowsService(WinServiceConfig.InstallName,
                ToInstallParameters(WinServiceConfig, install, serviceStartup));
        }

        public void UninstallFromWindowsServices()
        {
            if (WinServiceHelpers.IsServiceInstalled(WinServiceConfig.InstallName))
            {
                WinServiceHelpers.UninstallWindowsService(WinServiceConfig.InstallName,
                    ToInstallParameters(WinServiceConfig));
            }
        }
      
        public void StartInstalledWindowsService()
        {
            WinServiceHelpers.StartService(WinServiceConfig.InstallName);
        }
        
        public void SetRecoveryRestartForWindowsService(int delay = 30000)
        {
            WinServiceHelpers.SetRecoveryRestart(WinServiceConfig.InstallName, delay);
        }

        #endregion

        #region Convert Params

        private const string ServiceUserParam = "serviceUser";
        private const string ServicePasswordParam = "servicePassword";
        private const string ServiceAccountParam = "serviceAccount";
        private const string ServiceDisplayNameParam = "serviceDisplayName";

        public IEnumerable<string> ToInstallParameters(IWindowsServiceConfig winServiceConfig, string install = null,
            string serviceStartup = null)
        {
            if (winServiceConfig.Username != null && winServiceConfig.Account.ServiceAccount() == ServiceAccount.User)
                yield return string.Format("/{0}={1}", ServiceUserParam, winServiceConfig.Username);
            if (winServiceConfig.Password != null && winServiceConfig.Account.ServiceAccount() == ServiceAccount.User)
                yield return string.Format("/{0}={1}", ServicePasswordParam, winServiceConfig.Password);

            yield return string.Format("/{0}={1}", ServiceAccountParam, winServiceConfig.Account);

            if (winServiceConfig.DisplayName != null)
                yield return string.Format("/{0}={1}", ServiceDisplayNameParam, winServiceConfig.DisplayName);

            if (install != null)
                yield return string.Format("/install=\"{0}\"", install);

            if (serviceStartup != null)
                yield return string.Format("/serviceStartup=\"{0}\"", serviceStartup);
        }

        public void ParseContext(IWindowsServiceConfig winServiceConfig, StringDictionary parameters, out string install,
            out string serviceStartup)
        {
            if (!string.IsNullOrEmpty(getValue(parameters, ServiceUserParam)))
                winServiceConfig.Username = getValue(parameters, ServiceUserParam);

            if (!string.IsNullOrEmpty(getValue(parameters, ServicePasswordParam)))
                winServiceConfig.Password = getValue(parameters, ServicePasswordParam);

            if (!string.IsNullOrEmpty(getValue(parameters, ServiceDisplayNameParam)))
                winServiceConfig.DisplayName = getValue(parameters, ServiceDisplayNameParam);


            string accountParam = getValue(parameters, ServiceAccountParam);
            WinServiceAccount newAcc;
            if (accountParam != null && Enum.TryParse(accountParam, out newAcc))
            {
                winServiceConfig.Account = newAcc;
            }
            else
            {
                winServiceConfig.Account = WinServiceAccount.LocalSystem;
            }

            install = getValue(parameters, "install");
            serviceStartup = getValue(parameters, "serviceStartup");
        }

        private string getValue(StringDictionary parameters, string name)
        {
            if (parameters.ContainsKey(name))
            {
                string result = parameters[name];
                if (String.IsNullOrWhiteSpace(result)) return null;
                return result;
            }
            return null;
        }

        #endregion
    }

    /// <summary>
    ///     For test
    /// </summary>
    internal static class AssemblyHelper
    {
        private static Assembly _entryAssembly = Assembly.GetEntryAssembly();

        public static Assembly EntryAssembly
        {
            get { return _entryAssembly; }
            set { _entryAssembly = value; }
        }
    }
}