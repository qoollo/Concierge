#region

using System;
using System.Diagnostics.Contracts;
using System.Management;
using System.ServiceProcess;

#endregion

namespace Qoollo.Concierge.WindowsService
{
    internal class WinServiceStatus
    {
        public readonly string ServiceName;
        private string _statusString;

        public WinServiceStatus(string serviceName)
        {
            ServiceName = serviceName;
        }

        public bool IsInstalled { get; protected set; }
        public string StartName { get; protected set; }
        public string Description { get; protected set; }
        public ServiceControllerStatus State { get; protected set; }
        public string DisplayName { get; protected set; }

        public static WinServiceStatus Get(string serviceName)
        {
            Contract.Requires(serviceName != null);
            var result = new WinServiceStatus(serviceName);

            ServiceController serviceController = WinServiceHelpers.GetServiceController(serviceName);
            if (serviceController != null)
            {
                result.IsInstalled = true;
                result.DisplayName = serviceController.DisplayName;
                result.State = serviceController.Status;
            }

            // Query WMI for additional information about this service. 
            // Display the start name (LocalSytem, etc) and the service 
            // description.
            using (var wmiService = new ManagementObject("Win32_Service.Name='" + serviceName + "'"))
            {
                try
                {
                    wmiService.Get();
                    result.Description = wmiService["Description"].ToString();
                    result.StartName = wmiService["StartName"].ToString();
                }
                catch (Exception e)
                {
                    // Нам здесь пофиг на эксепшены
                }
            }

            return result;
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return _statusString ?? (_statusString = String.Format(
                "\n === SERVICE {0} STATUS: {1}  \n === Display Name: {2}; State: {3}; Start Name: {4}, Description: {5}",
                ServiceName, (IsInstalled) ? "INSTALLED" : "NOT INSTALLED", DisplayName, State, StartName, Description));
        }
    }
}