﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using Qoollo.Concierge;
using Qoollo.Concierge.Attributes;
using Qoollo.Concierge.Extensions;
using Qoollo.Concierge.WindowsService;

namespace TestProject
{
    public class Program
    {
        private static void Main(string[] args)
        {

            var user = new UserExecutor {Message = "OLOLOLOLO"};

            var appBuilder = new AppBuilder(true, user);

            appBuilder.AddStartupParameter("-s", () => { });
            appBuilder.AddCommand("get", () =>
            {
                throw new NotImplementedException();
            });

            appBuilder.WithWinServiceProps(t => t.Async = true)
                .WithWinServiceProps(t => t.DisplayName = "QoolloEmptyService")
                .WithWinServiceProps(t => t.InstallName = "QoolloEmptyService")
                .WithWinServiceProps(
                    t =>
                        t.Description =
                            "This service is test only. Qoollo provides you with great open source .NET solutions.")
                .WithWinServiceProps(t => t.StartAfterInstall = true)
                .EnableControlCommands()
                .EnableInfoCommands()
                .LogToFile()
                .UseExecutor(t =>
                {
                    t.OnStart(user.Start);
                    t.OnStop(user.Stop);
                })
                .Run(args);
        }
    }

    public class UserExecutor : InstallerBase, IUserExecutable
    {
        private bool _stop;
        public string Message = "Working";

        public void Start()
        {            
            _stop = false;
            Console.WriteLine("Starting");
            for (int i = 0; i < 100000; i++)
            {
                Console.WriteLine(Message);
                WriteMessage(@"M:\tmp\tmp.txt", i.ToString());
                Thread.Sleep(2000);
                if (_stop)
                    break;
            }
        }

        public void Stop()
        {
            _stop = true;
        }

        [StartupParametersHandler]
        public void StartupMethod(string[] keys)
        {            
        }

        public IWindowsServiceConfig Configuration
        {
            get { return new WinServiceConfig(); }
        }

        public static void WriteMessage(string file, string message, bool append = true)
        {
            using (var stream = new StreamWriter(file, append))
            {
                stream.WriteLine(message);
            }
        }
    }
}