using System;
using Qoollo.Concierge.UniversalExecution;
using Qoollo.Concierge.UniversalExecution.Core;
using Qoollo.Concierge.WindowsService;

namespace Qoollo.Concierge.Samples
{
    public class SampleProgramSimple
    {
        private static void SampleMain(string[] args)
        {
            var helper = new AppBuilder();

            helper.AddStartupParameter("-s", () => Console.WriteLine("You type s in args"));
            helper.AddCommand("comm", () => "You enter comm");

            helper.Run(args);

            // OR FAST RUN WITHOUT COMMANDS
            // AppBuilder.FastRun(true, args);
        }

       

        public class InstallableExecutor : IUserExecutable
        {
            private readonly IWindowsServiceConfig _winServiceConfig = new WinServiceConfig()
            {
                Async = true,
                Description = "beautiful service",
                DisplayName = "Qoollo Concierge Sample",
                InstallName = "QoolloConciergeSample",
                StartAfterInstall = true,
                Password = "Password",
                RestartOnRecover = true,
                Username = @".\ConciergeUser"
            };

            public void Start()
            {
                try
                {
                    //TODO: Build your system
                    //TODO: Start your system
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            public void Stop()
            {
                try
                {
                    //TODO: Stop your system
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            public IWindowsServiceConfig Configuration
            {
                get { return _winServiceConfig; }
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}