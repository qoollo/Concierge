using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using Qoollo.Concierge.Logger;
using Qoollo.Concierge.UniversalExecution.AppModes;

namespace Qoollo.Concierge.Extensions
{
    public static class AppBuilderRemoteControlExtension
    {
        /// <summary>
        /// Command for control IUserExecutable (Stop, Start Restart) in Debug and WinService mode
        /// </summary>
        /// <param name="appBuilder"></param>
        /// <returns></returns>
        public static AppBuilder EnableControlCommands(this AppBuilder appBuilder)
        {
            appBuilder.GetMode<DebugMode>("debug")
                .SetIExecutableWrapper(executable => new ExecutableControl(executable, appBuilder.Logger));

            appBuilder.AddCommand<DebugMode>("stop", (mode, name, args) => Stop(mode), "Stop executor");
            appBuilder.AddCommand<DebugMode>("start", (mode, name, args) => Start(mode), "Start executor");
            appBuilder.AddCommand<DebugMode>("restart", (mode, name, args) => Restart(mode), "Restart executor");

            appBuilder.GetMode<WinServiceMode>("win")
                .SetIExecutableWrapper(executable => new ExecutableControl(executable, appBuilder.Logger));

            appBuilder.AddCommand<WinServiceMode>("stop", (mode, name, args) => Stop(mode), "Stop executor");
            appBuilder.AddCommand<WinServiceMode>("start", (mode, name, args) => Start(mode), "Start executor");
            appBuilder.AddCommand<WinServiceMode>("restart", (mode, name, args) => Restart(mode), "Restart executor");

            return appBuilder;
        }

        /// <summary>
        /// Help commands (Ping, Config, Status, Version) in Debug and WinService modes
        /// </summary>
        /// <param name="appBuilder"></param>
        /// <returns></returns>
        public static AppBuilder EnableInfoCommands(this AppBuilder appBuilder)
        {
            appBuilder.GetMode<DebugMode>("debug")
                .SetIExecutableWrapper(executable => new ExecutableControl(executable, appBuilder.Logger));

            appBuilder.GetMode<WinServiceMode>("win")
                .SetIExecutableWrapper(executable => new ExecutableControl(executable, appBuilder.Logger));

            appBuilder.AddCommand<DebugMode>("ping", (mode, name, args) => "I am alive", "Ping programm");
            appBuilder.AddCommand<DebugMode>("config", (mode, name, args) => GetConfig(), "Show config");
            appBuilder.AddCommand<DebugMode>("status", (mode, name, args) => "Current mode is Debug", "Get current mode");
            appBuilder.AddCommand<DebugMode>("version", (mode, name, args) => "Current version is " + Version(),
                "Get current version");

            appBuilder.AddCommand<WinServiceMode>("ping", (mode, name, args) => "I am alive", "Ping programm");
            appBuilder.AddCommand<WinServiceMode>("config", (mode, name, args) => GetConfig(), "Show config");
            appBuilder.AddCommand<WinServiceMode>("status", (mode, name, args) => "Current mode is Service",
                "Get current mode");
            appBuilder.AddCommand<WinServiceMode>("version", (mode, name, args) => "Current version is " + Version(),
                "Get current version");

            return appBuilder;
        }

        /// <summary>
        /// Log errors in file
        /// </summary>
        /// <param name="appBuilder"></param>
        /// <param name="filePath">Path to the file</param>
        /// <returns></returns>
        public static AppBuilder LogToFile(this AppBuilder appBuilder, string filePath = "")
        {
            appBuilder.Logger.AddLogger(new FileLogger(filePath));
            return appBuilder;
        }

        private static string Version()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion;
        }

        private static string Stop(AppMode mode)
        {
            var executable = mode.FindDecorator<ExecutableControl>();
            if (executable != null) executable.CommandStop();
            return HttpStatusCode.OK.ToString();
        }

        private static string Start(AppMode mode)
        {
            var executable = mode.FindDecorator<ExecutableControl>();
            if (executable != null) executable.CommandStart();
            return HttpStatusCode.OK.ToString();
        }

        private static string Restart(AppMode mode)
        {
            var executable = mode.FindDecorator<ExecutableControl>();
            if (executable != null) executable.CommandRestart();
            return HttpStatusCode.OK.ToString();
        }

        private static string GetConfig()
        {
            string str;
            try
            {
                using (StreamReader file = File.OpenText(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile))
                {
                    str = file.ReadToEnd();
                }
            }
            catch (ArgumentException e)
            {
                str = e.Message;
            }
            catch (DirectoryNotFoundException e)
            {
                str = e.Message;
            }
            catch (IOException e)
            {
                str = e.Message;
            }
            catch (UnauthorizedAccessException e)
            {
                str = e.Message;
            }
            catch (NotSupportedException e)
            {
                str = e.Message;
            }
            return str;
        }
    }
}