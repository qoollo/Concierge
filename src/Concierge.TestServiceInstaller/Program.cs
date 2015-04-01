using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qoollo.Concierge;
using Qoollo.Concierge.Extensions;
using Qoollo.Concierge.WindowsService;

namespace Concierge.TestServiceInstaller
{
   public  class Program
    {

        static void Main(string[] args)
        {
            var appBuilder = new AppBuilder(true, typeof(UserExecutor));
           
            appBuilder.WithWinServiceProps(t => t.Async = true)
                .WithWinServiceProps(t => t.DisplayName = "QoolloEmptyServiceTest")
                .WithWinServiceProps(t => t.InstallName = "QoolloEmptyServiceTest")
                .WithWinServiceProps(
                    t =>
                        t.Description =
                            "This service is test only.")
                .WithWinServiceProps(t => t.StartAfterInstall = true)
                .EnableControlCommands()
                .EnableInfoCommands()
                .Run(args);
        }
    }

    public static class ExecutionHelper
    {
        public static string GetExePath()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }
    }

    public class TestInstaller : InstallerBase
    {
    }
}
