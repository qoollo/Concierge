using System;
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
    public class Program
    {
        private static void Main(string[] args)
        {
            var appBuilder = new AppBuilder(true, typeof (UserExecutor))
                .LogToFile(@"M:\tmp\log.txt");
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
            _stop = false;
            Console.WriteLine("Starting");
            int t = DateTime.Now.Millisecond;
            for (int i = 0; i < 100000; i++)
            {
                Console.WriteLine(Message);
                WriteMessage(string.Format( @"M:\tmp\tmp{0}.txt", t), i.ToString());
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
            get
            {
                return new WinServiceConfig
                {
                    InstallName = "Test",
                    DisplayName = "Test"
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