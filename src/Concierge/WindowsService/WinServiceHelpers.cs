using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Management;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace Qoollo.Concierge.WindowsService
{
    internal static class WinServiceHelpers
    {
        /// <summary>
        ///  Install WinService
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="args"></param>
        /// <param name="timeout"></param>
        public static void InstallWindowsService(string serviceName, IEnumerable<string> args, int timeout)
        {
            InstallService(serviceName, false, AssemblyHelper.EntryAssembly.Location, args, timeout);
        }

        /// <summary>
        ///  Uninstall WinService
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="args"></param>
        /// <param name="timeout"></param>
        public static void UninstallWindowsService(string serviceName, IEnumerable<string> args, int timeout)
        {
            InstallService(serviceName, true, Assembly.GetEntryAssembly().Location, args, timeout);
        }
        

        public static bool IsServiceInstalled(string name)
        {
            if (name == null) return false;
            return ServiceController.GetServices().Any(s => s.ServiceName == name);
        }

        public static bool IsServiceStopped(string name)
        {
            if (name == null) return false;
            var service = GetServiceController(name);
            if (service == null)
                return false;

            return service.Status == ServiceControllerStatus.Stopped;
        }

        public static ServiceController GetServiceController(string name)
        {
            if (name == null) return null;
            ServiceController result = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == name);
            return result;
        }

        public static void UninstallviaWmi(string name)
        {
            string objPath = string.Format("Win32_Service.Name='{0}'", name);
            using (var service = new ManagementObject(new ManagementPath(objPath)))
            {
                try
                {
                    ManagementBaseObject outParams = service.InvokeMethod("delete", null, null);
                    Console.WriteLine(outParams["ReturnValue"].ToString());
                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        ///  Start installed WinService
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="timeoutMilliseconds"></param>
        public static void StartService(string serviceName, int timeoutMilliseconds = 30000)
        {
            if (!IsServiceInstalled(serviceName))
                return;

            ServiceController service = GetServiceController(serviceName);

            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, timeout);
        }

        public static void StopService(string serviceName, int timeoutMilliseconds = 30000)
        {
            if (!IsServiceInstalled(serviceName))
                return;

            ServiceController service = GetServiceController(serviceName);

            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
        }

        public static void RestartService(string serviceName, int timeoutMilliseconds = 30000)
        {
            if (!IsServiceInstalled(serviceName))
                return;

            StopService(serviceName, timeoutMilliseconds);
            StartService(serviceName, timeoutMilliseconds);
        }
        public static void InstallService(string name, bool uninstall, string exePath, IEnumerable<string> args, int timeout)
        {
            Contract.Requires(name != null && exePath != null);
            timeout = timeout <= 0 ? -1 : timeout;
            bool serviceExists = IsServiceInstalled(name);
            List<string> fullArgs = (args != null) ? args.ToList() : new List<string>(2);

            if (uninstall) // UnInstall
            {
                try
                {
                    fullArgs.Add("/u");
                    fullArgs.Add(exePath); // must be the last argument!!!
                    // via sc.exe
                    ManagedInstallerClass.InstallHelper(fullArgs.ToArray());
                }
                finally
                {
                    UninstallviaWmi(name);
                }

                int timer = 0;
                while (IsServiceInstalled(name) && (timeout > timer || timeout == -1))
                {
                    Console.WriteLine("Waiting for serivce to uninstall");
                    Thread.Sleep(1000);
                    timer += 1000;
                }
            }
            else // Install
            {
                fullArgs.Add(exePath); // must be the last!!! ( как можно было так писать парсер :( )
                if (serviceExists)
                    throw new InstallException("Failed to install service. Service already exists.");
                ManagedInstallerClass.InstallHelper(fullArgs.ToArray());

                int timer = 0;
                while (!IsServiceInstalled(name) && (timeout > timer || timeout == -1))
                {
                    Console.WriteLine("Waiting for serivce to install");
                    Thread.Sleep(1000);
                    timer += 1000;
                }
            }
        }

        public static void SetRecoveryRestart(string serviceName, int delay)
        {
            int exitCode;
            using (var process = new Process())
            {
                ProcessStartInfo startInfo = process.StartInfo;
                startInfo.FileName = "sc";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                // tell Windows that the service should restart if it fails
                startInfo.Arguments = string.Format("failure \"{0}\" reset= 0 actions= restart/{1}", serviceName, delay);

                process.Start();
                process.WaitForExit();

                exitCode = process.ExitCode;
            }

            if (exitCode != 0)
                throw new Exception("Set recovery restart failed with code = " + exitCode);
        }
    }
}