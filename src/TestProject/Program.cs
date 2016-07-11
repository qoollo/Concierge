using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Qoollo.Concierge;
using Qoollo.Concierge.Attributes;
using Qoollo.Concierge.Commands;
using Qoollo.Concierge.Extensions;
using Qoollo.Concierge.WindowsService;

namespace TestProject
{
    static class Messages
    {
        public static string LogPath = @"M:\tmp\concierge\log.txt";
        public static string LogPathService = @"M:\tmp\concierge\log_service.txt";
    }

    public class Program
    {
        private static void Main(string[] args)
        {
            var appBuilder = new AppBuilder(true, typeof (UserExecutor))
                .LogToFile(Messages.LogPath).EnableInfoCommands();
            appBuilder.AddStartupParameter("serv", value =>
            {
                appBuilder.WindowsServiceConfig.DisplayName = value;
                appBuilder.WindowsServiceConfig.InstallName = value;
            }, "Change service name");

            appBuilder.Run(args);
        }
    }

    public class MyCommand:UserCommand
    {
        [Parameter(ShortKey = 'p', Description = "123213213")]
        public int Port { get; set; }
    }

    public class UserExecutor : InstallerBase, IUserExecutable
    {
        private bool _stop;
        public string Message = "Working";

        [CommandHandler("command")]
        public string Command(MyCommand command)
        {
            return "Ok";
        }

        public void Start()
        {
            Thread.Sleep(2000);
            throw new NotImplementedException();
            _stop = false;
            Console.WriteLine("Starting");
            for (int i = 0; i < 100000; i++)
            {
                Console.WriteLine(Message);
                WriteMessage(Messages.LogPathService, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                Thread.Sleep(1000);
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
            get
            {
                return new WinServiceConfig
                {
                    InstallName = "Test",
                    DisplayName = "Test",
                    Async = true
                };
            }
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