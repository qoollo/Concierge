using System;
using System.Configuration.Install;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

namespace Qoollo.Concierge.UniversalExecution.Core
{
    internal class InstallersManager
    {
        private readonly Assembly _assembly;
        private readonly Installer _installer;

        private ServiceProcessInstaller _processInstaller;
        private ServiceInstaller _serviceInstaller;

        public InstallersManager(Installer installer, Assembly assembly)
        {
            Contract.Requires(installer != null);
            Contract.Requires(assembly != null);
            _installer = installer;
            _assembly = assembly;
            CreateInstallers();
        }

        private void CreateInstallers()
        {
            _processInstaller = new ServiceProcessInstaller();
            _serviceInstaller = new ServiceInstaller();

            _installer.Installers.Add(_processInstaller);
            _installer.Installers.Add(_serviceInstaller);
        }

        private void SetContextParameter(string key, string value)
        {
            if (!_installer.Context.Parameters.ContainsKey(key))
                _installer.Context.Parameters.Add(key, value);
            else
                _installer.Context.Parameters[key] = value;
        }

        public void UpdateInstallersFromConfig(IWindowsServiceConfig config)
        {
            _processInstaller.Account = config.Account.ServiceAccount();

            if (!String.IsNullOrWhiteSpace(config.Username))
            {
                SetContextParameter("user", config.Username);
                _processInstaller.Username = config.Username;
                _processInstaller.Account = ServiceAccount.User;
            }
            if (!String.IsNullOrWhiteSpace(config.Password))
            {
                _processInstaller.Password = config.Password;
            }

            _serviceInstaller.ServiceName = config.InstallName;
            _serviceInstaller.DisplayName = config.DisplayName;
            _serviceInstaller.Description =
                config.DisplayName + ". Version: " + _assembly.GetName().Version + ". " +
                config.Description;
            _serviceInstaller.StartType = ServiceStartMode.Automatic;
        }

        /// <summary>
        /// Create service starup arguments
        /// </summary>
        /// <param name="install"></param>
        /// <param name="startupParameter"></param>
        public void SetServiceStartParameters(string install, string startupParameter)
        {
            const string startParameters = " :win";

            var path = new StringBuilder(_installer.Context.Parameters["assemblypath"]);
            if (path[0] != '"')
            {
                path.Insert(0, '"');
                path.Append('"');
            }

            if (startupParameter != "\"\"")
            {
                startupParameter = startupParameter.Remove(startupParameter.Length - 1, 1).Remove(0, 1);
                foreach (string value in startupParameter.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries))
                {
                    path.Append(" " + value);
                }
            }

            path.Append(" " + startParameters);

            if (install != null)
            {
                path.Append(" " + install);
            }

            _installer.Context.Parameters["assemblypath"] = path.ToString();
        }        
    }
}